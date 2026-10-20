#!/usr/bin/env python3
"""
Scaffold a new solution with the CleanSolution architecture.

Copies assets/skeleton into <output>/<name>, keeps only the selected optional modules,
generates the .slnx and appsettings files, then builds the solution.

Usage:
    python scaffold.py --name InventoryHub --output D:/Projects
    python scaffold.py --name InventoryHub --output D:/Projects --modules messaging,worker --jwt

Modules (the core is always included: Core.Shared, Core.Domain, Core.Application,
Infrastructure.Persistence, Presentation.WebApi):
    messaging  -> Infrastructure.Messaging   (Kafka, typed HttpClients, correlation id forwarding)
    documents  -> Infrastructure.Documents   (MinIO file storage)
    worker     -> Presentation.Worker        (cron-scheduled background jobs, SlimTaskScheduler)
    mcp        -> Presentation.Mcp           (Model Context Protocol server exposing use cases as tools)

Flags:
    --jwt       wire JWT authentication + fallback authorization policy (code is always present, off by default)
    --no-build  skip `dotnet build`
    --force     allow a non-empty target directory
"""
from __future__ import annotations

import argparse
import json
import re
import secrets
import shutil
import subprocess
import sys
import uuid
from pathlib import Path

SKELETON = Path(__file__).resolve().parent.parent / "assets" / "skeleton"

OPTIONAL_MODULES = {
    "messaging": "Infrastructure/Infrastructure.Messaging",
    "documents": "Infrastructure/Infrastructure.Documents",
    "worker": "Presentation/Presentation.Worker",
    "mcp": "Presentation/Presentation.Mcp",
}

CORE_PROJECTS = [
    "Core/Core.Application",
    "Core/Core.Domain",
    "Core/Core.Shared",
    "Infrastructure/Infrastructure.Persistence",
    "Presentation/Presentation.WebApi",
]

TEXT_SUFFIXES = {".cs", ".csproj", ".json", ".xml", ".md", ".http", ".props", ".targets"}

IF_RE = re.compile(r"^\s*(?://|<!--)\s*@if\s+(!?)(\w+)\s*(?:-->)?\s*$")
ENDIF_RE = re.compile(r"^\s*(?://|<!--)\s*@endif\s*(?:-->)?\s*$")
MODULE_RE = re.compile(r"^\s*//\s*@module\s+(\w+)\s*$")


def fail(message: str) -> None:
    print(f"ERROR: {message}", file=sys.stderr)
    sys.exit(1)


def process_text(text: str, flags: set[str]) -> str | None:
    """Apply @module / @if / @endif markers. Returns None when the whole file must be skipped."""
    lines = text.splitlines(keepends=True)

    if lines:
        module = MODULE_RE.match(lines[0])
        if module:
            if module.group(1) not in flags:
                return None
            lines = lines[1:]

    output: list[str] = []
    stack: list[bool] = []  # True = the block is active
    for number, line in enumerate(lines, start=1):
        start = IF_RE.match(line)
        if start:
            negate, flag = start.group(1) == "!", start.group(2)
            active = (flag in flags) != negate
            stack.append(active and all(stack))
            continue
        if ENDIF_RE.match(line):
            if not stack:
                raise ValueError(f"unbalanced @endif at line {number}")
            stack.pop()
            continue
        if all(stack):
            output.append(line)

    if stack:
        raise ValueError("unclosed @if block")
    return "".join(output)


def project_module(relative: Path) -> str | None:
    posix = relative.as_posix()
    for module, folder in OPTIONAL_MODULES.items():
        if posix.startswith(folder + "/"):
            return module
    return None


def copy_skeleton(target: Path, name: str, flags: set[str]) -> list[Path]:
    written: list[Path] = []
    for source in sorted(SKELETON.rglob("*")):
        if source.is_dir():
            continue
        relative = source.relative_to(SKELETON)
        module = project_module(relative)
        if module and module not in flags:
            continue

        destination = target / relative
        if source.suffix.lower() in TEXT_SUFFIXES:
            text = source.read_text(encoding="utf-8")
            try:
                processed = process_text(text, flags)
            except ValueError as error:
                fail(f"{relative}: {error}")
            if processed is None:
                continue
            processed = processed.replace("__SOLUTION_NAME__", name)
            if "__USER_SECRETS_ID__" in processed:
                processed = processed.replace("__USER_SECRETS_ID__", str(uuid.uuid4()))
            destination.parent.mkdir(parents=True, exist_ok=True)
            destination.write_text(processed, encoding="utf-8", newline="\n")
        else:
            destination.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(source, destination)
        written.append(destination)
    return written


def write_json(path: Path, data: dict) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8", newline="\n")


def kebab(name: str) -> str:
    return re.sub(r"[^a-z0-9-]", "-", name.lower()).strip("-") or "app"


def write_appsettings(target: Path, name: str, flags: set[str]) -> None:
    connection = f"Server=localhost;Database={name};Trusted_Connection=True;TrustServerCertificate=True;"
    logging = {"LogLevel": {"Default": "Information", "Microsoft.AspNetCore": "Warning"}}

    # ---------- WebApi ----------
    web = target / "Presentation" / "Presentation.WebApi"
    write_json(web / "appsettings.json", {
        "ApiKey": "",
        "JwtSettings": {"Issuer": name, "Audience": name, "Key": ""},
        "Cors": {"Origins": [], "ExposedHeaders": []},
        "IpRateLimiting": {
            "EnableEndpointRateLimiting": True,
            "StackBlockedRequests": False,
            "RealIPHeader": "X-Real-IP",
            "ClientIdHeader": "X-ClientId",
            "HttpStatusCode": 429,
            "GeneralRules": [{"Endpoint": "*", "Period": "1s", "Limit": 50}],
        },
        "AllowedHosts": "*",
    })
    write_json(web / "appsettings.Production.json", {})
    write_json(web / "appsettings.Staging.json", {})

    development: dict = {
        "Serilog": {
            "Using": ["Serilog.Sinks.Seq", "Serilog.Sinks.Graylog"],
            "MinimumLevel": {
                "Default": "Information",
                "Override": {"Microsoft": "Error", "System": "Warning", "Microsoft.Hosting.Lifetime": "Error"},
            },
            "WriteTo": [
                {"Name": "Seq", "Args": {"ServerUrl": "http://localhost:5341", "Period": "00:00:10"}},
                {"Name": "Graylog", "Args": {"hostnameOrAddress": "localhost", "port": 12201, "transportType": "Udp"}},
            ],
        },
        "ConnectionStrings": {"DefaultConnection": connection},
        # development-only values; real ones come from user-secrets / environment variables
        "ApiKey": secrets.token_urlsafe(24),
        "JwtSettings": {"Key": secrets.token_urlsafe(48)},
        "Cors": {"Origins": ["http://localhost:4200", "https://localhost:4200"]},
    }
    # every host composes the same infrastructure layers, so every host needs their sections
    module_sections: dict = {}
    if "messaging" in flags:
        module_sections["ApacheKafka"] = {"BootstrapServers": "localhost:9092", "Group": f"{kebab(name)}-group"}
        module_sections["ExternalServices"] = {}
    if "documents" in flags:
        module_sections["MinioConfigs"] = {
            "MinioBucket": {"BucketName": f"{kebab(name)}-docs"},
            "MinioClient": {"Endpoint": "localhost:9000", "AccessKey": "minioadmin", "SecretKey": "minioadmin"},
        }
    development.update(module_sections)
    write_json(web / "appsettings.Development.json", development)
    (web / "wwwroot").mkdir(parents=True, exist_ok=True)

    # ---------- Worker ----------
    if "worker" in flags:
        worker = target / "Presentation" / "Presentation.Worker"
        write_json(worker / "appsettings.json", {
            "CronExpression": "*/5 * * * *",
            "Logging": {"LogLevel": {"Default": "Information", "Microsoft.Hosting.Lifetime": "Information"}},
        })
        write_json(worker / "appsettings.Development.json", {
            "ConnectionStrings": {"DefaultConnection": connection},
            "Logging": {"LogLevel": {"Default": "Debug", "Microsoft.Hosting.Lifetime": "Information"}},
            **module_sections,
        })

    # ---------- MCP ----------
    if "mcp" in flags:
        mcp = target / "Presentation" / "Presentation.Mcp"
        write_json(mcp / "appsettings.json", {"Logging": logging, "AllowedHosts": "*"})
        write_json(mcp / "appsettings.Development.json", {
            "ConnectionStrings": {"DefaultConnection": connection},
            **module_sections,
        })


def write_slnx(target: Path, name: str, flags: set[str]) -> Path:
    projects = CORE_PROJECTS + [folder for module, folder in OPTIONAL_MODULES.items() if module in flags]
    groups: dict[str, list[str]] = {"Core": [], "Infrastructure": [], "Presentation": []}
    for folder in sorted(projects):
        layer, project = folder.split("/")
        groups[layer].append(f"{folder}/{project}.csproj")

    lines = ["<Solution>"]
    for layer in ("Core", "Infrastructure", "Presentation"):
        lines.append(f'  <Folder Name="/{layer}/">')
        lines += [f'    <Project Path="{path}" />' for path in groups[layer]]
        lines.append("  </Folder>")
    lines += ['  <Folder Name="/Solution Items/">', '    <File Path="README.md" />', "  </Folder>", "</Solution>", ""]

    slnx = target / f"{name}.slnx"
    slnx.write_text("\n".join(lines), encoding="utf-8", newline="\n")
    return slnx


def write_readme(target: Path, name: str, flags: set[str]) -> None:
    modules = ", ".join(sorted(flags - {"jwt"})) or "none"
    text = f"""# {name}

.NET 10 solution following the CleanSolution (Clean Architecture) template.

Optional modules: {modules}. JWT: {"enabled" if "jwt" in flags else "available, not enabled"}.

## Run

```bash
dotnet build {name}.slnx
dotnet run --project Presentation/Presentation.WebApi
```

API reference (Development): `/scalar/v1`, `/swagger`. Health: `/health`.

## Database migrations

```bash
dotnet ef migrations add <Name> --project Infrastructure/Infrastructure.Persistence --startup-project Presentation/Presentation.WebApi --output-dir Migrations
dotnet ef database update --project Infrastructure/Infrastructure.Persistence --startup-project Presentation/Presentation.WebApi
```

Secrets (connection strings with passwords, `ApiKey`, `JwtSettings:Key`, broker/storage credentials) belong in
`dotnet user-secrets` or environment variables, never in committed appsettings files.
"""
    (target / "README.md").write_text(text, encoding="utf-8", newline="\n")


def run(command: list[str], cwd: Path) -> subprocess.CompletedProcess:
    return subprocess.run(command, cwd=cwd, capture_output=True, text=True, encoding="utf-8", errors="replace")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--name", required=True, help="Solution name, e.g. InventoryHub")
    parser.add_argument("--output", default=".", help="Parent directory; the solution goes to <output>/<name>")
    parser.add_argument("--modules", default="", help="Comma-separated: messaging,documents,worker,mcp (or 'all')")
    parser.add_argument("--jwt", action="store_true", help="Enable JWT authentication + fallback policy")
    parser.add_argument("--no-build", action="store_true", help="Skip dotnet build")
    parser.add_argument("--force", action="store_true", help="Allow a non-empty target directory")
    args = parser.parse_args()

    if not re.fullmatch(r"[A-Za-z_][A-Za-z0-9_.]*", args.name):
        fail("--name must be a valid identifier (letters, digits, '_' or '.'), e.g. InventoryHub")

    requested = {m.strip().lower() for m in args.modules.split(",") if m.strip()}
    if "all" in requested:
        requested = set(OPTIONAL_MODULES)
    unknown = requested - set(OPTIONAL_MODULES)
    if unknown:
        fail(f"unknown module(s): {', '.join(sorted(unknown))}. Allowed: {', '.join(OPTIONAL_MODULES)}")

    flags = set(requested)
    if args.jwt:
        flags.add("jwt")

    if not SKELETON.is_dir():
        fail(f"skeleton not found at {SKELETON}")

    target = Path(args.output).resolve() / args.name
    if target.exists() and any(target.iterdir()) and not args.force:
        fail(f"{target} is not empty (use --force to scaffold into it anyway)")
    target.mkdir(parents=True, exist_ok=True)

    written = copy_skeleton(target, args.name, flags)
    write_appsettings(target, args.name, flags)
    slnx = write_slnx(target, args.name, flags)
    write_readme(target, args.name, flags)

    if shutil.which("dotnet"):
        gitignore = run(["dotnet", "new", "gitignore"], target)
        if gitignore.returncode != 0:
            print("warning: `dotnet new gitignore` failed; add a .gitignore manually", file=sys.stderr)

    print(f"Scaffolded {args.name} at {target}")
    print(f"  solution : {slnx.name}")
    print(f"  modules  : {', '.join(sorted(requested)) or 'core only'}")
    print(f"  jwt      : {'enabled' if args.jwt else 'available, not enabled'}")
    print(f"  files    : {len(written)} from skeleton")

    if args.no_build:
        return
    if not shutil.which("dotnet"):
        fail("dotnet SDK not found on PATH; install .NET 10 SDK and run `dotnet build`")

    build = run(["dotnet", "build", slnx.name, "-nologo", "-v", "q"], target)
    tail = "\n".join((build.stdout + build.stderr).strip().splitlines()[-15:])
    if build.returncode != 0:
        print(tail)
        fail("dotnet build failed")
    print("  build    : succeeded")


if __name__ == "__main__":
    main()
