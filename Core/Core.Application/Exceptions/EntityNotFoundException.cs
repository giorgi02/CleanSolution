﻿namespace Core.Application.Exceptions;
public sealed class EntityNotFoundException : EntityValidationException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;

    /// <summary>
    /// მოთხოვნილი ჩანაწერი ვერ მოიძებნა
    /// </summary>
    public EntityNotFoundException(string message) : base(message) { }
}