
📘 How to Merge develop → main Using a Pull Request (PR)
This guide explains how to merge the develop branch into main using a Pull Request.
This is the recommended workflow because it keeps the history clean and ensures the automated release system works correctly.

🧭 1. Push the latest changes on develop
Before creating a PR, make sure your develop branch is up to date on GitHub.

In your terminal:

Code
git checkout develop
git push origin develop
If everything is already pushed, Git will say everything is up to date.

🧭 2. Open the Pull Request on GitHub
Go to your repository on GitHub.

GitHub will usually show a banner:

“Compare & pull request”

If you see it, click it.

If not, you can manually create a PR:

Click Pull requests

Click New pull request

Set:

Base branch: main

Compare branch: develop

GitHub will now show all the changes that will be merged.

🧭 3. Review the changes
GitHub displays a diff of everything that changed between the two branches.

Check that:

The new installer files are present

The old WiX files are removed

Any app changes you made are correct

This is your chance to confirm everything looks right.

🧭 4. Create the Pull Request
Click:

Create pull request

Give it a title, for example:

Merge develop into main

You can leave the description empty or add notes if you want.

🧭 5. Merge the Pull Request
Once the PR is created, you’ll see a green button:

Merge pull request

Click it, then click:

Confirm merge

Your develop branch is now merged into main.

🧭 6. What happens next (automated release pipeline)
After the merge, your new automation takes over:

✔ Step 1 — release-please runs
It scans the changes on main and creates a release PR.

This PR will have a title like:

chore: release 0.6.0

It also includes:

A version bump

A generated changelog

✔ Step 2 — You merge the release PR
When you merge this PR, GitHub automatically:

Creates a GitHub Release

Creates a version tag (e.g., v0.6.0)

Triggers the MSI build workflow

✔ Step 3 — The MSI installer is built
Your workflow:

Publishes the app

Builds the MSI using WixSharp

Uploads the MSI to the GitHub Release

You’ll see the installer appear under the release assets.

🧭 7. Summary
Using a PR to merge develop → main ensures:

Clean history

Predictable automation

Correct versioning

Automatic changelog generation

Automatic MSI builds

A fully modern release pipeline

This is the recommended workflow for all future releases.


============================================================
This method just replaces main with develop - no messy Pull requests
In your terminal:

Code
git checkout develop
git push origin develop

2. On GitHub, go to:
Settings → Branches → Default branch

Change the default branch from:

main → develop

(This avoids GitHub shouting at you when you rename branches.)

3. Rename develop to main
On GitHub:

Go to Branches

Find develop

Click the pencil icon next to it

Rename it to:

Code
main