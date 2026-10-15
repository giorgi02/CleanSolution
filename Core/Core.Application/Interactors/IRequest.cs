namespace Core.Application.Interactors;

// მარკერ ინტერფეისები — runtime-ზე არაფერს აკეთებს, მხოლოდ request-ის ტიპს აღნიშნავს

public interface IRequest { }

public interface IRequest<out TResponse> { }
