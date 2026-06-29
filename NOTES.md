kom_rank in segment effort could be null - that means segment has been created after activity has been recorded - nice information to show

Agreement:
- 7 days means to refresh cache: https://groups.google.com/g/strava-api/c/yP8tV9KapZs

# Features

Ranking by:
- total koms lenght
- total elevation gain
- avg speed
- ...

Arena | Battle field:
- competition between athlets
- who bet who etc.

King of Cracow etc.
- top x segment in area
- who has the most of them

Koms map

Koms by type on dashboard

Last changes new koms, lost koms for following, club etc.

Segment stars tracking - notify when someone stared segment that you have kom.

Segment direction and sorting.

# Extended KOM classes for 0 category 
# based on https://www.marcellobrivio.com/projects/strava-toolbox/kom-lister.php
# https://support.strava.com/hc/en-us/articles/216917057-Climb-Categorization

DH:
D1 - Steep DH: avg < -8%
D2 - Standard DH: -8% <= avg < -4%

SP - Sprint: 4% <= avg < 3%, dist < 1000

Flat (less than 3% because greater or equals are counted to category > 0):
FM - Flat: 4% <= avg < 3%, 1000 <= dist < 3500
TT1 - Short TT: 4% <= avg < 3%, 3500 <= dist < 7500
TT2 - Long TT: 4% <= avg < 3%, 7500 <= dist

Climbs:
WL - Wall: 8% <= avg
MC - Minnor climb: 3% <= avg < 8%

# Clubs

In club edit page you can set only club type.
Fields in api: `activity_types`, `activity_types_icon` are set bassed on club type.

# Logo

Font: Gill Sans Ultra Bold

# Fixes

## 2026-06-27 clear invalid data after Strava API glitch

### Issue

Sometimes the Strava API returns an empty list of KOMs (instead of throwing an error). It’s even worse when it returns only a partial list (e.g., half of them). This messes up the statistics because suddenly a user loses all their KOMs, only to regain them on the next API call.

### Observations from historical data

- Losses below 20 KOMs are usually legitimate (e.g., KOMs lost to a user driving a car, regained later after the ride is flagged).
- Large quantitative losses (e.g., 150+ lost) can be legitimate if the user has thousands of KOMs (e.g., during Strava's periodic segment cleanups).
- Partial API failures result in an unnaturally high percentage of lost KOMs (often 50-80% of the user's total count).

### Current temporary solution

For now, I have cleaned up the data using the following criteria:
```
delete 
from public.koms_summary_segment_effort
where koms_summary_id in 
(
	select id from public.koms_summary
	where lost_koms > 100 or returned_koms > 100
)

delete from public.koms_summary
where lost_koms > 100 or returned_koms > 100
```

Additionally, I reviewed cases (>10) and manually removed the ones that didn't look right:
```
delete from public.koms_summary_segment_effort
where koms_summary_id in (4064,4001,27562,4034,3971,4010,3947,27586,27576,4056,3993,4049,3986,4042,3979,4036,3973,28424,28421,4031,3968,27585,27575,4046,3983,4016,3953,4008,3945,27580,27570,4045,3982,4020,3957,4017,3954,27584,27574,4035,3972,4065,4002,4040,3977,4025,3962,4022,3959,4063,4000,4054,3991,4030,3967,4009,3946,4026,3963,4013,3948,4050,3987,28447,28446,4051,3988,4055,3992,19722,19720,4019,3956,4044,3981)

delete from public.koms_summary
where id in(4064,4001,27562,4034,3971,4010,3947,27586,27576,4056,3993,4049,3986,4042,3979,4036,3973,28424,28421,4031,3968,27585,27575,4046,3983,4016,3953,4008,3945,27580,27570,4045,3982,4020,3957,4017,3954,27584,27574,4035,3972,4065,4002,4040,3977,4025,3962,4022,3959,4063,4000,4054,3991,4030,3967,4009,3946,4026,3963,4013,3948,4050,3987,28447,28446,4051,3988,4055,3992,19722,19720,4019,3956,4044,3981)
```

### Observations

For losses below 20, I saw a lot of legitimate cases—for example, KOMs lost because someone drove a car over the segment, which were later regained after the ride was flagged.

### DONE: Implement "Smart" Safeguard

We need to implement a safeguard to skip updates when the API returns suspicious data.
Let previous_koms = the number of KOMs from the previous fetch.

Skip the update if either of the following conditions is met:

Empty list response:
`koms == 0 AND lost_koms > 20`

Partial list response (API truncation):
`koms > 0 AND lost_koms > 50 AND (lost_koms / previous_koms) > 0.35`

(Note: The 35% ratio threshold should safely allow for large, legitimate segment cleanups, which typically represent a small percentage of a user's total, while catching major API partial drops.)

