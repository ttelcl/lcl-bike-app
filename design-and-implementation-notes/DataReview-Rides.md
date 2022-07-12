# Rides Data Review

## Introduction

Before starting to use the data and start coding, let's first take a 
step back and see if there anything relevant we can learn about the data. The
assignment wisely calls for a data validation step while importing data into
the app and suggests dropping out any records for rides shorter than 10 meters.
Is that enough? Are there other validation steps that would make sense?

### In this document:

* [Step 1: look at distributions](#step-1-look-at-distributions)
* [Step 2: checking the times](#step-2-checking-the-times)
* [Step 3: checking uniqueness](#step-3-checking-uniqueness)
* [Step 4: timestamp encoding](#step-4-timestamp-encoding)

## Step 1: look at distributions

One thing I have found useful in the past is to plot the distributions 
of data; in the case of the bike ride data that applies to the distance and 
duration of the rides. So I calculated and visualized those "distrigrams" using
some tools that I had lying around. Normally I would not store images like this
in a GIT repository, but I think that in this case it is educational.

![Cumulative distrigrams of distance and duration](distrigrams-full.png)

There are several ways to visualize distribution plots. My prefered way
of visualizing these (demonstrated above) has the following properties:

* The X-axis shows the percentile axis (so: the range is from 0% to 100%)
* The Y-axis shows the value for the variable (distance or duration, with
separate graph lines for each of the months).

Here are two ways to think about these such plots:
* One interpretation is that they are simply all data samples sorted by value, and then
scaled to fit an X-axis running from 0 to 100.
* Another interpretation is to think of them is as a cumulative distribution, flipped
around the diagonal (the two axes turned around).

Having the percentile axis horizontal makes it easier to stack multiple independent
plots vertically (like in this case the plots for distance and duration)

You can easily read, for example, the median distance and duration from the plots: that's
the values at 50. You can see that the median distance for all three months is just above
2000m. And the median duration is about 660 seconds.

So why show this plot? Well, there are some things visible that may inform what
a decent data validation stage could filter out. Under normal circumstances I
would suggest discussing these things with the customer, but I guess that in
this case I just have to make decisions by myself. Here are a few observations
and some conclusions I drew.

* _Please note that I scaled the plots so you get a decent overview, but that means
some of the top and bottom gets cut off._
* The plots for the three data months are very similar. For the distance distribution
plots you can barely see that there are in fact three plots: they are mostly just
on top of each other.
* There is something suspicious going on at the start of the distributions.
    * For the distance, there are actual a handful of rows with "distance" being
    very large negative values (order of magnitude: 4000 km negative). It would
    make a good question to ask the customer about: is it just a bug? Some
    hardware issue? Hackers? A side effect of no location data available? No
    matter what, these values of course should not be taken into account.
    * Then there are quite many cases of "distance=0". The exercise instructions
    clearly state those should be skipped, as well as any other values below 10 m, 
    but I would like to understand what would be a good cutoff. What is exactly
    the purpose of the app? By dropping the "0 m" rides you forego the chance
    on providing insights on what exactly happens for those. And in reverse, if
    you want to gain insights based on "normal" data, isn't that 10m cutoff
    actually far too low? In the graph above you can see that the "main" part
    of the distribution only starts for rides above about 400m or so. 
    * Similar considerations can be applied to the low end of the "duration"
    distribution. One curiosity is that there fewer abnormal samples for 
    duration than for distance. If you want a safe "cut-off" for minimal
    duration of "normal" rides I would suggest 120 seconds.
* There are also suspicious things at the high end of the distributions as 
well, and I would suggest to include upper bounds for duration and distance.
    * Setting a duration cutoff at 4 hours (14400 seconds) would only exclude the
    top 0.30% of the rides, and most of those are probably not going to be
    representative of "normal" rides (and more cases of "oops, forgot to return").
    In fact, in the duration graph you can just see that there seems to be a 
    steepening of the graph already around 1800 seconds (30 minutes). If you
    would cut off there, you would still only exclude less than 5% of the rides.
    * Similarly for an upper distance cap - you would only lose about 1% of the data
    by dropping any rides longer than 8000 m.
* In addition to these value bounds, there are also some other checks. In particular,
some distance value are just blank.

## Step 2: checking the times

Since start and stop times are given for the ride, what is the point in also
including the duration of the ride in the data? You can calculate the duration from
these two times, isn't it? Well ... it turns out that the duration 
calculated from the start and end times does not exactly match the given
duration. Sometimes it is a few seconds short, sometimes a few seconds
too long.

I do not know why these durations are different, I recommend discussing
this matter with the customer. For now it seems that it is _not_ a good
idea to throw away the given duration value during data import to
conserve space...

Taking a closer look at the durations as given and the durations as calculated:
it appears that it is rare (< 2%) that these two durations differ by more than 20
seconds, but if they do they may differ by many hours. As said, I don't know what
exactly is happening, but my gut feeling says to ignore any rides where the 
given and calculated durations differ by more than 20 seconds.

## Step 3: checking uniqueness

It would be nice if there were a way to derive a unique identifier for each ride,
such that if you attempt to import the same ride again you can detect that.

First, just to get a feeling, lets calculate how many rows typically occur for each
distinct departure time. 

It turns out that that analysis serendipuously brought another issue to the light!
Each timestamp occurs 2, 4, 6, 8, 10, 12, 14 or 16 times. Notice something
conspicuous? It turns out that each data file contains two copies of each ride
record. It is one of those things that aren't obvious immediately, but once you
know what to look for it is easy to verify: load a data file in an editor or
text viewer, copy the first data line, and search for it again (paste the copied
line in the search box). It turns out that it appears again right in the middle
of the file: the second half of the file is an exact copy of the first half
(minus the header line).

Let's see if we can find a combination of columns in the ride data that is
unique for each ride (except that we know that each ride is duplicated). Such
a combination could be used to derive a key column that uniquely identifies
the ride.

I tried several subsets of columns that failed for this purpose. The names
of the two stations can be omitted (since they are 1-to-1 dependent on the
station IDs), but other columns seem required to create a robustly unique
identifier for the row. It is probably best to let the database generate
a row ID upon insertion, but use a UNIQUE index on all columns except names
to ensure no duplicates are inserted.

## Step 4: timestamp encoding

While looking at the ride data I noticed one other peculiarity that is important
while parsing the timestamps in the data files: almost all of the timestamps
use the "yyyy-MM-dd'T'HH:mm:ss" (ISO) format. However, for timestamps that are
exactly at the start of a day, "yyyy-MM-dd" is used (with no time part at all).
