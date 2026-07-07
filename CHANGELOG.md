# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [1.0.0] - 2022-01-01

### Added

- State, StateMachine got added

## [2.0.0] - 2024-11-18

### Added
- StateParameters can now be used to add large number of states in a single call

### Changed

- StateTransition has been moved to the Specialised namespace and commented out for now
- TransitionalStateMachine has been moved to the Specialised namespace and commented out for now
- State now uses Update over OnTick
- State now uses Exit over OnExit
- State now uses Enter over OnEnter

### Removed

- Tests have been removed due to large changes in the system
- Removed IState interface

## [2.1.0] - 2024-11-19

### Changed

- Updated dependency of bbunity-test-support to 2.1.0

## [3.0.0] - 2026-04-29

### Added

- `StateMachine.HasState(string key)` — returns true if a state is registered under that key
- `StateMachine.RemoveState(string key)` — removes a registered state; throws `KeyNotFoundException` if the key is absent
- `StateMachine.OnStateChanged` — event fired after every transition, passing the previous state (null on first transition) and the new state
- `StateMachine.FixedUpdate()` and `StateMachine.LateUpdate()` — physics-step and post-render tick methods, mirroring `Update()`
- `State.FixedUpdate()` and `State.LateUpdate()` — empty virtual methods so subclasses only override what they need
- Comprehensive test suite covering `StateMachine`, `State`, `StateParameter`, `StateParameters`, and `CallbackState`
- XML documentation summaries on `StateParameter` and `StateParameters`
- Thread safety notice added to README

### Changed

- **Breaking** — `State._stateMachine` field changed from `protected` to `private`; subclasses must use the `StateMachine` property instead
- `StateMachine` internal dictionary changed from `Dictionary<int, State>` (keyed by hash code) to `Dictionary<string, State>`, eliminating any risk of hash collision silently overwriting a registered state
- `StateMachine.AddState()` now validates the `state` parameter and throws `ArgumentNullException` when null
- `StateMachine.SetState()` now validates the `key` parameter and throws `ArgumentNullException` when null
- `StateMachine.GetState()` now throws a descriptive `KeyNotFoundException` that includes the missing key name
- `StateMachine.ReplaceState()` now throws `ArgumentNullException` if `newState` is null, preventing the machine from silently entering an uninitialised state
- `StateParameters.Add(string, State)` now logs a `Debug.LogWarning` and ignores the entry when a duplicate key is detected, rather than silently overwriting
- `IEnumerable` implementation updated from `KeyValuePair<int, State>` to `KeyValuePair<string, State>` to match the dictionary change
- README rewritten with correct, working code examples covering all features

### Removed

- `TransitionalStateMachine` and `StateTransition` — both files contained only commented-out, incomplete code and have been deleted
- Inaccurate XML comment stating "StateMachine also acts as a State"
- Unresolved TODO comment in `State.cs`