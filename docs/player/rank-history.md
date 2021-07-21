[← Go Back](https://github.com/ewang2002/RealmEyeSharper/tree/master/RealmAspNet/docs/docs-guide.md)

# RealmEye API → Player API → Rank History
This endpoint gives you the data that is displayed on the player's rank history page.

## Endpoint

```
// Gets an unformatted JSON.
GET api/realmeye/player/rankhistory?name={name}

// Gets a formatted JSON.
GET api/realmeye/player/rankhistory?name={name}&prettify
```

### Arguments

| Argument Name | Type | Required? | Description |
| ---- | ---- | --------- | ----------- |
| `name` | `string` | Yes | The name of the player to request. This is case-insensitive. |
| `prettify` | | No | If included, returns a formatted JSON. |

## TypeScript Definition 
The corresponding TypeScript definition is:
```ts 
interface IRankHistory extends IRealmEyePlayerResponse {
    rankHistory: {
        rank: number;
        achieved: string;
        date: string;
    }[];
}
```

## Examples
Here are some examples. 

### Case 1 
```
GET api/realmeye/player/rankhistory?name=japan
```
Response:
```json
{"rankHistory":[{"rank":85,"achieved":"2021-07-02 09:55:38 in less than a minute","date":"2021-07-02T09:55:38Z"},{"rank":84,"achieved":"2021-07-02 09:55:07 in ~ 2 days 18 hours 31 minutes","date":"2021-07-02T09:55:07Z"},{"rank":85,"achieved":"2021-06-29 15:23:30 in ~ 3 hours 57 minutes","date":"2021-06-29T15:23:30Z"},{"rank":84,"achieved":"2021-06-29 11:26:03 in less than a minute","date":"2021-06-29T11:26:03Z"},{"rank":85,"achieved":"2021-06-29 11:25:48 in ~ 39 minutes","date":"2021-06-29T11:25:48Z"},{"rank":84,"achieved":"2021-06-29 10:46:09 in less than a minute","date":"2021-06-29T10:46:09Z"},{"rank":85,"achieved":"2021-06-29 10:45:38 in ~ 13 minutes","date":"2021-06-29T10:45:38Z"},{"rank":84,"achieved":"2021-06-29 10:32:36 in less than a minute","date":"2021-06-29T10:32:36Z"},{"rank":85,"achieved":"2021-06-29 10:32:30 in ~ 4 minutes","date":"2021-06-29T10:32:30Z"},{"rank":84,"achieved":"2021-06-29 10:27:44 in less than a minute","date":"2021-06-29T10:27:44Z"},{"rank":85,"achieved":"2021-06-29 10:27:35 in less than a minute","date":"2021-06-29T10:27:35Z"},{"rank":84,"achieved":"2021-06-29 10:27:10 in ~ 3 minutes","date":"2021-06-29T10:27:10Z"},{"rank":85,"achieved":"2021-06-29 10:24:04 in less than a minute","date":"2021-06-29T10:24:04Z"},{"rank":84,"achieved":"2021-06-29 10:23:51 in less than a minute","date":"2021-06-29T10:23:51Z"},{"rank":85,"achieved":"2021-06-29 10:23:40 in less than a minute","date":"2021-06-29T10:23:40Z"},{"rank":84,"achieved":"2021-06-29 10:23:21 in less than a minute","date":"2021-06-29T10:23:21Z"},{"rank":85,"achieved":"2021-06-29 10:22:27 in less than a minute","date":"2021-06-29T10:22:27Z"},{"rank":84,"achieved":"2021-06-29 10:22:20 in ~ 90 days 17 hours 59 minutes","date":"2021-06-29T10:22:20Z"},{"rank":85,"achieved":"2021-03-30 16:22:42 in ~ 314 days 2 hours 54 minutes","date":"2021-03-30T16:22:42Z"},{"rank":80,"achieved":"2020-05-20 13:28:06 in ~ 655 days 2 hours 51 minutes","date":"2020-05-20T13:28:06Z"},{"rank":75,"achieved":"2018-08-04 10:36:51 in ~ 2 days 23 hours 24 minutes","date":"2018-08-04T10:36:51Z"},{"rank":74,"achieved":"2018-08-01 11:12:26 in ~ 18 hours 10 minutes","date":"2018-08-01T11:12:26Z"},{"rank":73,"achieved":"2018-07-31 17:01:38 in ~ 57 minutes","date":"2018-07-31T17:01:38Z"},{"rank":72,"achieved":"2018-07-31 16:03:46","date":"2018-07-31T16:03:46Z"}],"profileIsPrivate":false,"sectionIsPrivate":false,"name":"japan","resultCode":200}
```

### Case 2
``` 
GET api/realmeye/player/rankhistory?name=Cube&prettify
```
Response: 
```json 
{
  "rankHistory": [
    {
      "rank": 85,
      "achieved": "2021-07-02 12:24:27 in ~ 5 minutes",
      "date": "2021-07-02T12:24:27Z"
    },
    {
      "rank": 84,
      "achieved": "2021-07-02 12:19:20 in ~ 8 minutes",
      "date": "2021-07-02T12:19:20Z"
    },
    {
      "rank": 83,
      "achieved": "2021-07-02 12:10:35 in ~ 8 minutes",
      "date": "2021-07-02T12:10:35Z"
    },
    {
      "rank": 82,
      "achieved": "2021-07-02 12:01:58 in ~ 1 minute",
      "date": "2021-07-02T12:01:58Z"
    },
    {
      "rank": 78,
      "achieved": "2021-07-02 12:00:26 in ~ 2 minutes",
      "date": "2021-07-02T12:00:26Z"
    },
    {
      "rank": 77,
      "achieved": "2021-07-02 11:58:08 in ~ 1 minute",
      "date": "2021-07-02T11:58:08Z"
    },
    {
      "rank": 76,
      "achieved": "2021-07-02 11:57:08 in ~ 2 days 16 hours 14 minutes",
      "date": "2021-07-02T11:57:08Z"
    },
    {
      "rank": 85,
      "achieved": "2021-06-29 19:42:40 in less than a minute",
      "date": "2021-06-29T19:42:40Z"
    },
    {
      "rank": 84,
      "achieved": "2021-06-29 19:41:51 in ~ 1 minute",
      "date": "2021-06-29T19:41:51Z"
    },
    // ... omitted some entries
    {
      "rank": 4,
      "achieved": "2015-07-25 21:07:34 in ~ 1 hour 30 minutes",
      "date": "2015-07-25T21:07:34Z"
    },
    {
      "rank": 3,
      "achieved": "2015-07-25 19:37:16 in ~ 7 days 9 hours 56 minutes",
      "date": "2015-07-25T19:37:16Z"
    },
    {
      "rank": 2,
      "achieved": "2015-07-18 09:40:44 in ~ 1 hour 54 minutes",
      "date": "2015-07-18T09:40:44Z"
    },
    {
      "rank": 1,
      "achieved": "2015-07-18 07:46:21",
      "date": "2015-07-18T07:46:21Z"
    }
  ],
  "profileIsPrivate": false,
  "sectionIsPrivate": false,
  "name": "cube",
  "resultCode": 200
}
```