# Security and privacy

## Local credential handling

CodexQuotaWidget does not bundle credentials and does not implement its own sign-in flow. To query reset-card expiry data, it reads the existing Codex `auth.json` at runtime and uses the access token only in memory for a direct request to ChatGPT. Tokens, account IDs, quota snapshots, and reset-card records are not written to application logs or settings.

The local settings file stores only UI preferences, window position, the nearest cached expiry time, and reminder state.

## Release hygiene

- `auth.json`, settings, logs, build folders, symbols, and generated release artifacts are excluded from Git.
- Release builds disable debug symbols and map build paths so binaries do not expose the developer machine's absolute paths.
- Published release archives should contain the application executable and checksum only; do not attach PDB files.

## Reporting a vulnerability

Please open a GitHub security advisory instead of posting credentials or private account data in a public issue.
