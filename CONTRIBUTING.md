
# Contributing to Calcita

Thank you for considering contributing to Calcita. We welcome contributions of all kinds — bug reports, documentation improvements, features and fixes.

Below are guidelines to make the review and merge process faster.

## Issues
- Bug reports: include reproduction steps, environment (OS, .NET SDK), expected and actual behavior, and a minimal reproduction if possible.
- Feature requests: describe the use case, proposed API/UX and backward-compatibility considerations.

## Pull requests

Before submitting a pull request:
- Base your work on the latest `main` branch and create a topic branch (e.g. `feature/xyz` or `fix/abc`).
- Keep commits focused and with clear messages. Recommended format: `type(scope): short description` (e.g. `fix(core): correct formula parsing`).
- Run `dotnet build` and ensure the solution compiles.
- Provide tests when applicable and update documentation if behavior changes.
- In the PR description include a summary, motivation, verification steps and any compatibility impact. If the PR fixes an issue, reference it (e.g. `Fixes #123`).

Code review notes:
- Follow the project's coding conventions (target: .NET 8, C# 14). Use existing file and naming styles.
- Public APIs should include XML documentation.

## License and attribution

This repository is a fork of ReoGrid and remains under the MIT License. By submitting a PR you agree to license your contributions under MIT unless otherwise stated.

Notes:
- Portions of the code originate from ReoGrid (https://reogrid.net). Original authors are credited in source files and the LICENSE.

## Privacy

Your GitHub username and email may appear in the repository's contributor list. If you prefer not to be listed, contact the maintainers before submitting.

Thank you for your contribution!
