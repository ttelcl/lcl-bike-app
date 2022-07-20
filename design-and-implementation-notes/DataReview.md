# Reviewing the data files

Before starting the actual coding it is useful to take a closer look at the data
files. This document only provides summaries and links to more in-depth analyses.

The aims of this analysis are:

* Derive data validation limits and rules for the ride data.
* Gain insights on data redundancy or non-redundancy useful for designing the
data model
* Check the relation between the station data and the ride data files, to
help discover potential pitfalls.

## Ride data validation limits and rules

For an in-depth analysis, see [Rides data review](DataReview-Rides.md).
Based on that analysis I came up with the following limits and rules.
Under normal circumstances such rules and limits should be discussed with
the customer of course. In particular, limits marked with (*) should be
discussed with the customer.

* Drop any duplicate data rows. As mentioned in the data analysis,
all data rows occur as two copies (once in the first half of the file,
once in the second half)
* Drop any rides with a blank value (no value) for "Covered Distance"
* Round the covered distance for all rides to full meters. There are
only a VERY few rides where that distance isn't in full meters already.
* (*) Drop any rides with a "Covered Distance" shorter than 400m. The exact
cut-off limit is open for discussion. This limit also implies the following
limits (which are probably not really open for discussion)
    * Drop any rides with a negative value for "Covered Distance"
    * Drop any rides with a 0 value for "Covered Distance"
* (*) Drop any rides with a "Covered Distance" above 8000m
* (*) Drop any rides with a Duration below 120 seconds 
* (*) Drop any rides with a Duration above 14400 seconds (4 hours)
* (*) Drop any rides where the specified duration is more than 20 seconds off
from the one you can calculate from departure and return times. Remember to
discuss why the "duration" values do not match the difference between
departure and return times to gain better insight in what these three values
actually are.

Not mentioned before, but a topic that I ran into when designing the database:
there is no natural primary key for each ride record. The best you can do is
adding a "unique" condition on all ride data fields in the CSV file. I would
recommend discussing with the customer if it would be possible to have the unique
identifier of the _bike_ that was used for the ride in the data (I assume they
have that information). That would allow constructing a unique ride ID by
combining that bike identifier with the departure time. Doing so would allow
a simple and efective protection against inserting duplicates of rides.

## Station IDs and names

For in-depth observations regarding the IDs and names of bike stations
see [Station IDs and names Review](DataReview-Station-Ids-Names.md).
Based on that review the following rules are suggested and the following
discussion topics are highlighted for customer discussion.
* Drop rides starting or ending at stations 999 and 997. These do not
have a presence in the station table. They are technical support stations,
not end-user stations.
    * Alternative: support stations with incomplete data. That would affect
    both database (nullability of columns), backend (nullability of field
    values), and frontend (rendering incomplete data)
* Clarify what is up with station 754 ("Lintumetsä"). Like the technical
stations above, this only appears in ride data, but not in the station
listing. This feels like an omission or error in station table.
* Clarify what is up with station 729 ("Leppävaaranaukio"). This station
jumps out because it is the only station that is never used in the ride
data, only in the station table. Could it be that 754 and 729 are
actually the same???
* For both ride data and station data, consider translating any
non-breaking space characters to normal spaces.
* Clarify what to do with cases where the station name in the ride
data is different from the one in the station table. Should these cases
be treated as alternate names for the station?
* Observation: the longest station name is
'Aalto-yliopisto (M), Korkeakouluaukio' (37 characters).


