# Observations when looking at station ids and names

## Methodology:

I looked at the IDs and names for the stations ocurring as departure and return station
for the rides, and tried matching them to the IDs and names in the station list.

## My observations:

* For the vast majority of bike ride entries the station id (for both departure and return)
can be matched with the "ID" column in the station list, and the station name in the
ride record matches the Finnish name of the station (column 'Nimi')
* With one exception, all stations that occur as "departure" also occur as "return"
at some point. The exception is station 999, named "Bike Production". That name already
explains why it is a source of rides only :)
* Stations 754 ('Lintumetsä'), 999 ('Bike production') and 997 ('Workshop Helsinki')
do not have an entry in the station list. For 999 and 997 that can be explained by
assuming they are technical support stations, not available to the general public.
The fact that their name is (exceptionally) in English already hints that they have
a special status.
The case of station 754 may require closer inspection and discussion with the customer.
* There is one station in the station table that is never used as return or departure
station: station 729 ('Leppävaaranaukio'). I wonder if it is somehow related to the
above-mentioned station 754 ('Lintumetsä') which is missing in the opposite direction.
* There are a handful of stations where the name of the station is different in
the ride tables and the station table:
    * These are the observed cases:
        * 541 'Aalto-yliopisto (M), Korkea' vs 'Aalto-yliopisto (M), Korkeakouluaukio'.
        * 539 'Aalto-yliopisto (M), Tietot' vs 'Aalto-yliopisto (M), Tietotie'
        * 735 'Armas Launiksen katu' vs 'Mestarinkatu'
        * 583 'Haukilahdenkatu' vs 'Haukilahdensolmu'
        * 769 'Tiurintie' vs 'Lumivaarantie'
        * 901 'O'Bike Station' vs 'Outotec'
        * 290 'Vallilan varikko' vs 'Vallilan varikko'
    * In two of these cases the name in the station table is truncated. In a few more
    cases the difference in names probably requires local knowledge to explain. For the
    "Vallilan varikko" case I had to resort to a hex viewer to figure out what was
    going on. It turns out that in one of the two cases the space between the words
    is actually a non-breaking space (Unicode \u00A0) instead of a normal one (\u0020)...

