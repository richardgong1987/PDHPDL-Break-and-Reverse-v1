# Testing Guide

Pure, framework-independent logic (e.g. `RiskUtil`) is unit-tested with **xUnit** under
`tests/`. The cBot itself is not unit-tested here — it is validated in cTrader's backtester.

## Why the test project is separate

- It targets **`net10.0`** (the installed runtime), not the cBot's `net6.0`.
- It is **not** part of `PDHPDL Break and Reverse v1.sln`, so cTrader never tries to build it.
- It **links** pure source files via `<Compile Include>` instead of referencing the cBot
  project, so tests never pull in the `cTrader.Automate` / cAlgo.API dependency.

## Prerequisites

- .NET SDK installed. Check with:

  ```bash
  dotnet --version
  ```

## One command: build + test

`scripts/test.sh` builds the cBot (Release) and runs the unit tests in one step. Run it from
anywhere — it resolves the repo root itself:

```bash
./scripts/test.sh
```

## Run all tests

To run only the tests, from the repository root:

```bash
dotnet test "tests/RiskUtil.Tests/RiskUtil.Tests.csproj"
```

Expected tail of the output:

```text
Passed!  - Failed: 0, Passed: 27, Skipped: 0, Total: 27
```

## Useful variations

```bash
# More detail per test
dotnet test "tests/RiskUtil.Tests/RiskUtil.Tests.csproj" -v normal

# List the test names without running them
dotnet test "tests/RiskUtil.Tests/RiskUtil.Tests.csproj" --list-tests

# Run one test class
dotnet test "tests/RiskUtil.Tests/RiskUtil.Tests.csproj" \
  --filter "FullyQualifiedName~CalcVolumeByRiskTests"

# Run one test method by name
dotnet test "tests/RiskUtil.Tests/RiskUtil.Tests.csproj" \
  --filter "Name=returns_zero_when_stop_equals_entry"
```

## Adding tests for new logic

1. Keep the new logic pure (no `cAlgo.API`, no I/O, no `DateTime.Now`).
2. If it lives in a new file, link it in `tests/RiskUtil.Tests/RiskUtil.Tests.csproj`:

   ```xml
   <Compile Include="..\..\PDHPDL Break and Reverse v1\Util\YourClass.cs" Link="YourClass.cs" />
   ```

3. Add a test class named after the unit under test; name tests by behavior, e.g.
   `rejects_settlement_before_trade_date`.
