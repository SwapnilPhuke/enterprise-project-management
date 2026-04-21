# Contributing Guide

Thank you for your interest in contributing to the Enterprise Project Management System! This guide will help you get started with development and contribution.

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally
3. **Create a feature branch** for your changes
4. **Follow the development setup** in the README.md

## Development Workflow

### Backend Development

1. Navigate to the backend directory
   ```bash
   cd backend
   ```

2. Make your changes to the code

3. Build the project
   ```bash
   dotnet build
   ```

4. Run the project
   ```bash
   dotnet run
   ```

5. Test your changes with the Swagger UI at `http://localhost:5000/swagger`

### Frontend Development

1. Navigate to the frontend directory
   ```bash
   cd frontend
   ```

2. Make your changes to the React components

3. Start the development server
   ```bash
   npm start
   ```

4. Open `http://localhost:3000` in your browser

5. The app will automatically reload when you save changes

## Code Style Guidelines

### C# (.NET)

- Follow Microsoft's C# Coding Conventions
- Use meaningful variable and method names
- Add XML documentation comments for public methods
- Use async/await for asynchronous operations
- Implement proper error handling

Example:
```csharp
/// <summary>
/// Gets a project by its ID
/// </summary>
/// <param name="id">The project ID</param>
/// <returns>The project if found, null otherwise</returns>
public Project? GetById(int id)
{
    return _context.Projects.FirstOrDefault(p => p.Id == id);
}
```

### JavaScript/React

- Use functional components with Hooks
- Use meaningful component and variable names
- Use arrow functions
- Add comments for complex logic
- Follow Airbnb JavaScript Style Guide

Example:
```javascript
const handleAddProject = async () => {
  // Validate form inputs
  if (!formValid) return;
  
  try {
    // Make API call
    const response = await axios.post(`${API_URL}/api/projects`, {
      name: name.trim(),
      description: description.trim(),
    });
    
    // Handle success
    setSuccess('Project created successfully!');
  } catch (error) {
    // Handle error
    setError(error.message);
  }
};
```

## Commit Message Guidelines

- Use present tense ("Add feature" not "Added feature")
- Use imperative mood ("Move cursor to..." not "Moves cursor to...")
- Limit the subject line to 50 characters
- Reference issues and pull requests liberally after the first line

Example:
```
Add project status filtering functionality

- Implement status filter dropdown in UI
- Add status filter logic to service layer
- Update API to support status filtering
- Add unit tests for status filtering

Closes #123
```

## Pull Request Process

1. Update the README.md with any new features or changes
2. Ensure your code builds without errors
3. Write clear and descriptive commit messages
4. Create a pull request with a detailed description
5. Request review from maintainers
6. Address any feedback or suggestions

## Reporting Issues

When reporting bugs, include:

- A clear description of the issue
- Steps to reproduce the bug
- Expected behavior
- Actual behavior
- Screenshots (if applicable)
- Your environment (OS, browser, .NET version, etc.)

## Feature Requests

When suggesting new features:

- Explain the use case and why you believe it's important
- Provide examples of how the feature would work
- Discuss potential implementation approaches

## Questions?

Feel free to open an issue with the label `question` if you have any questions about the project or development process.

## Code Review

All submissions require review. We use GitHub pull requests for this purpose. Consult GitHub Help for more information on using pull requests.

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
