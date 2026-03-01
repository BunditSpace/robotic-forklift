# Setup and Test Instructions

This document provides step-by-step instructions for installing dependencies, running, and testing both the .NET Backend and Angular Frontend environments.

## Prerequisites

Before running setup, make sure the following are installed:

- Node.js: `20.x` or later (recommended: latest LTS)
- npm: `11.8.0` or later (`Frontend/package.json` uses `npm@11.8.0`)
- .NET SDK: `8.0.x` (Backend targets `net8.0`)
- Angular CLI: `21.1.4` or later

You can verify installed versions with:

```bash
node -v
npm -v
dotnet --version
ng version
```

## Backend (.NET 8)

### 1. Install Dependencies

Switch to the `Backend` directory and restore NuGet packages:

```bash
cd Backend
dotnet restore
```

### 2. Run the Application

Start the `.NET 8` API by targeting the `Forklift.WebAPI` project:

```bash
cd Backend
dotnet run --project Forklift.WebAPI/Forklift.WebAPI.csproj
```

_(The API will launch and generally output the localhost endpoint, such as `http://localhost:5164`)_

### 3. Run Tests

The project contains unit tests for Domain, Application, and API. To run all of them:

```bash
cd Backend
dotnet test
```

## Frontend (Angular) version 21.1.4

The frontend is built with Angular

### 1. Install Dependencies

Switch to the `Frontend` directory and install the necessary Node.js packages:

```bash
cd Frontend
npm install
```

### 2. Run the Application

Launch the Angular development server:

```bash
cd Frontend
npm start
```

_(The UI will be accessible locally at `http://localhost:4200`)_

### 3. Run Tests

**Unit Tests (Vitest):**
To execute component tests:

```bash
cd Frontend
npm test
```

**End-To-End Tests (Playwright):**
To run Playwright UI tests:

```bash
cd E2E
npx playwright test
```

To view the generated test report:

```bash
cd E2E
npx playwright show-report
```
