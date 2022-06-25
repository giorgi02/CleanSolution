namespace Core.Domain.Enums;
[Flags]
public enum Language : byte
{
    None = 0,

    Georgian = 1,
    English = 2,
    Russian = 4,
    German = 8,
    French = 16,
    Chinese = 32,

    All = Georgian + English + Russian + German + French + Chinese
}