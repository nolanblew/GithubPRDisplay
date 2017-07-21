# Github Display

This will show PRs for a repo (currently hard-coded) and a CR, Testing, Done flow.

Currently, this is for private use.

## Features
 - Toast Notifications for Devs (PR is done, or has issues)
 - Toast Notifications for QA when a PR enters a branch
 
## Future Goals
I would like to eventually integrate this with JIRA, get better state management, and a more customized workflow.
Also, I would like to convert this to MVVM and add tests. Any feature should be tracked with an issue.

## Known Issues
 - Currently using a custom dll for Github's Octokit.NET, in order to enable the viewing of reviews. This is
 currently preventing this app from passing store cert.
 - Toasts will not show when the app is closed or suspended.
