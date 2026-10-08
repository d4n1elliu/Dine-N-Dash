# Contributing Guidelines

Guidelines for contributing to this project. Please follow these to keep our workflow clean and reviewable.

## Issues

- Every piece of work needs an issue. Create one before starting if it doesn't exist.
- Assign the issue before starting work to avoid doubling up.
- Add one type label and at least one area label, such as `enhancement` and `backend`.

## Branches

- Create one branch per issue. Don't combine multiple features in a single branch.
- **Only create a branch when work on it is starting**, so it's based on the latest `main`. Pull `main` first.
- Never commit directly to `main`.
- Use the format `type/issue-number-short-description`
  - Examples: `feature/14-login-page`, `fix/22-search-filter`, `chore/13-api-setup`, `test/23-search-tests`

## Commits

- Use the format `type(scope): short description`, with `backend` or `web` as the scope
  - Examples: `feat(backend): add restaurant search endpoint`, `fix(web): stop empty booking dates`
- Write in the present tense, such as "add" instead of "added".
- Add `Refs #13` to the description to link the commit to its issue.

## Pull Requests

- Open one PR per issue. Don't group multiple features into one PR.
- Keep PRs focused and small. Split a branch up if it grows too large.
- Every PR needs **at least 1 review from a teammate** and a CodeRabbit check before merging.
- Delete the branch after it's merged into `main`.

### PR Title Format

Use the format `type(scope): short description`

- **type** is one of `feat`, `fix`, `chore`, `docs`, `refactor`, `test`, `style`
- **scope** is `backend` or `web`

Examples

- `feat(backend): create account endpoint`
- `feat(web): restaurant search page`
- `fix(web): login validation error`
- `chore(backend): update dependencies`

### PR Description

```markdown
@coderabbitai summary

## How to test
1. Steps to check the change works

Closes #13
```

`Closes #13` closes the issue automatically when the PR is merged.

### Before Opening a PR

- `dotnet build` and `dotnet test` pass
- `npm run lint` passes in `apps/web`
- The change works when the app is running
- No `.env` files, passwords or API keys are committed

## Code

- Backend logic goes in `DineNDash.Core/Services`, not in controllers.
- Use clear names, such as `RestaurantService` instead of `RS`.
- Add a short comment above any method that isn't obvious.
- Use Tailwind classes for frontend styling.
- Store secrets in .NET User Secrets, never in `appsettings.json`.

## Deployment

- The app is deployed on a VPS instead of Microsoft Azure.
- Only code merged into `main` gets deployed.
- Never commit server passwords, SSH keys or VPS details to the repo.

## Assignment Requirements

Mention any of these in the PR description when a change covers them, so they can be tracked.

- Polymorphism, interfaces, generics, LINQ with lambdas, enums and extension methods
- Entity Framework, NUnit tests, error handling and input validation
- At least four distinct responsive screens and six types of UI elements

## Build Requirements

- The project targets **.NET 10 (LTS)** and builds in **Visual Studio 2026**.
- Run `dotnet build` and `dotnet test` before every merge so `main` always builds and runs.
- Fix any build errors or warnings before opening a PR.