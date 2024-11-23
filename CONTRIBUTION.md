# Contribution Guidelines for Commit Messages

To maintain a clean and structured Git history,
this project follows the **Conventional Commits**
specification. When contributing to this project,
please ensure that your commit messages adhere
to the following format:

## Commit Message Format

Each commit message should consist of three parts:
1. **Type**: The type of change (e.g., `feat`, `fix`, etc.).
2. **Scope (optional)**: The area of the codebase affected by the change.
3. **Description**: A short, imperative description of the change.

### Format Example
```
<type>(<scope>): <description>

<body> (optional)
```

### Example
```
feat(auth): add user authentication service

- Implemented `AuthService` to handle user authentication.
- Added models for `AuthRequest` and `AuthResponse`.
- Configured API versioning and CORS for authentication endpoints.
```

## Allowed Commit Types

| **Type**      | **Description**                                                                 |
|---------------|---------------------------------------------------------------------------------|
| `feat`        | Introduces a new feature.                                                      |
| `fix`         | Fixes a bug or issue.                                                         |
| `docs`        | Updates documentation (e.g., README, inline comments).                        |
| `style`       | Changes code formatting or styling (no logic changes).                        |
| `refactor`    | Refactors code without changing functionality.                                 |
| `test`        | Adds or modifies tests.                                                       |
| `chore`       | Miscellaneous tasks (e.g., updating dependencies, build scripts).             |

## Detailed Guidelines

**Type and Scope**
   - Use one of the allowed types listed above.
   - Optionally include a scope in parentheses to specify the area of the codebase
     affected (e.g., `auth`, `docs`, `api`).

**Description**
   - Write a short, imperative description of what the commit does.
   - Use present tense (e.g., "add", "fix", "update") rather than past tense.

**Body (Optional)**
   - If necessary, provide additional context or details about the change in the body
     of the commit message.
   - Use bullet points for clarity if listing multiple changes.

**Breaking Changes**
   - If your commit introduces a breaking change, include a clear note in the body:
     ```
     BREAKING CHANGE: <explanation>
     ```

**Examples**
   - Adding a new feature:
     ```
     feat(auth): add user authentication service

     - Implemented `AuthService` to handle user authentication.
     - Added models for `AuthRequest` and `AuthResponse`.
     ```
   - Fixing a bug:
     ```
     fix(api): resolve null reference exception in AuthController
     ```
   - Updating documentation:
     ```
     docs(readme): update installation instructions
     ```

## Why Follow These Guidelines?

**Readable Git History**:
   - A consistent format makes it easier to understand what changes were made and why.

**Semantic Versioning**:
   - Conventional Commits enable tools like `semantic-release` to automatically determine
     version updates (`major`, `minor`, or `patch`) based on commit messages.

**Collaboration**:
   - Clear commit messages help team members and contributors quickly understand changes.

## Additional Notes

- If you're unsure about how to format your commit message, refer to these guidelines or
  ask for help in the project discussion channels.
- For more information on Conventional Commits, visit [https://www.conventionalcommits.org/](https://www.conventionalcommits.org/).

## Thank You for Contributing!

By following these guidelines, you help ensure that this project remains maintainable and
easy to collaborate on. Your contributions are greatly appreciated!
