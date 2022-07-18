This is a dummy file to ensure the folder it lives in is properly
persisted in GIT.

Note that this folder (including this file) is copied by the project's
post-build event.

Content:
- rides-subset.csv
	A few rows selected from the original ride data files.
	The rows were selected to include some "normal" cases but
	also some of the problematic cases discovered during the data
	review.
	
	Note that, like the original, some of the lines are repeated
	toward the end of the file. To conserve space not ALL of the
	earlier rows are repeated (like they are in the originals).

	Note that in one case a name is spelled with a non-breaking
	space; if you look at it in VS Code you may see it highlighted.

- stations-subset.csv
	A subset of the bike stations data file. In principle all
	stations referenced in rides-subset.csv are included, if they
	were available.

	Note that some of the rides were included just *because* they
	reference a station not present in the station table
