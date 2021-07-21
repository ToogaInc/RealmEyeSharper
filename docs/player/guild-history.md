[← Go Back](https://github.com/ewang2002/RealmEyeSharper/blob/master/docs/docs-guide.md)

# RealmEye API → Player API → Guild History
This endpoint gives you the data that is displayed on the player's guild history page.

## Endpoint

```
// Gets an unformatted JSON.
GET api/realmeye/player/guildhistory?name={name}

// Gets a formatted JSON.
GET api/realmeye/player/guildhistory?name={name}&prettify
```

### Arguments

| Argument Name | Type | Required? | Description |
| ---- | ---- | --------- | ----------- |
| `name` | `string` | Yes | The name of the player to request. This is case-insensitive. |
| `prettify` | | No | If included, returns a formatted JSON. |

## TypeScript Definition 
The corresponding TypeScript definition is:
```ts 
interface IGuildHistory extends IRealmEyePlayerResponse {
    guildHistory: {
        guildName: string;
        guildRank: string;
        from: string;
        to: string;
    }[];
}
```

## Examples
Here are some examples. 

### Case 1 
```
GET api/realmeye/player/guildhistory?name=opre
```
Response:
```json
{"guildHistory":[{"guildName":"Exotics","guildRank":"Founder","from":"2019-01-10T03:10:54Z","to":""},{"guildName":"Exotics","guildRank":"Leader","from":"2018-12-17T05:02:11Z","to":"2019-01-10T03:10:54Z"},{"guildName":"Exotics","guildRank":"Initiate","from":"2018-11-11T06:13:26Z","to":"2018-12-17T05:02:11Z"},{"guildName":"Not in a guild","guildRank":"","from":"2018-11-11T06:11:44Z","to":"2018-11-11T06:13:26Z"},{"guildName":"Banished Gods","guildRank":"Initiate","from":"2018-02-18T05:55:57Z","to":"2018-11-11T06:11:44Z"},{"guildName":"Not in a guild","guildRank":"","from":"2018-02-17T23:22:32Z","to":"2018-02-18T05:55:57Z"},{"guildName":"Common","guildRank":"Founder","from":"2017-08-09T03:04:31Z","to":"2018-02-17T23:22:32Z"},{"guildName":"Not in a guild","guildRank":"","from":"2017-08-04T18:21:12Z","to":"2017-08-09T03:04:31Z"},{"guildName":"Zexy Kendo Sticks","guildRank":"Officer","from":"2017-07-01T02:22:58Z","to":"2017-08-04T18:21:12Z"},{"guildName":"Zexy Kendo Sticks","guildRank":"Member","from":"2017-06-17T21:58:46Z","to":"2017-07-01T02:22:58Z"},{"guildName":"Zexy Kendo Sticks","guildRank":"Initiate","from":"2017-06-11T03:25:36Z","to":"2017-06-17T21:58:46Z"},{"guildName":"Not in a guild","guildRank":"","from":"2017-05-22T00:07:07Z","to":"2017-06-11T03:25:36Z"},{"guildName":"Exotics","guildRank":"Initiate","from":"2017-05-05T04:12:32Z","to":"2017-05-22T00:07:07Z"},{"guildName":"Not in a guild","guildRank":"","from":"2017-04-02T21:02:49Z","to":"2017-05-05T04:12:32Z"},{"guildName":"Loner Guild","guildRank":"Founder","from":"2016-03-01T00:59:21Z","to":"2017-04-02T21:02:49Z"},{"guildName":"Not in a guild","guildRank":"","from":"2016-03-01T00:59:07Z","to":"2016-03-01T00:59:21Z"},{"guildName":"HarpersFerry","guildRank":"Initiate","from":"2016-03-01T00:48:18Z","to":"2016-03-01T00:59:07Z"},{"guildName":"Not in a guild","guildRank":"","from":"2015-08-20T01:07:30Z","to":"2016-03-01T00:48:18Z"},{"guildName":"Sweet Games","guildRank":"Founder","from":"2015-06-02T22:19:35Z","to":"2015-08-20T01:07:30Z"},{"guildName":"Not in a guild","guildRank":"","from":"2015-06-02T22:19:02Z","to":"2015-06-02T22:19:35Z"},{"guildName":"LeechesOnDaBeaches","guildRank":"Leader","from":"2015-05-30T03:25:37Z","to":"2015-06-02T22:19:02Z"},{"guildName":"LeechesOnDaBeaches","guildRank":"Initiate","from":"2014-07-09T22:01:31Z","to":"2015-05-30T03:25:37Z"},{"guildName":"Not in a guild","guildRank":"","from":"2014-07-09T20:07:29Z","to":"2014-07-09T22:01:31Z"},{"guildName":"chasing legends","guildRank":"Initiate","from":"2014-01-27T22:21:46Z","to":"2014-07-09T20:07:29Z"},{"guildName":"Not in a guild","guildRank":"","from":"2013-08-25T01:28:31Z","to":"2014-01-27T22:21:46Z"},{"guildName":"FFA Tombs","guildRank":"Member","from":"2013-08-09T19:54:27Z","to":"2013-08-25T01:28:31Z"},{"guildName":"FFA Tombs","guildRank":"Initiate","from":"2013-08-09T19:03:03Z","to":"2013-08-09T19:54:27Z"},{"guildName":"Oryx Warriors","guildRank":"Founder","from":"","to":"2013-08-09T19:03:03Z"}],"profileIsPrivate":false,"sectionIsPrivate":false,"name":"opre","resultCode":200}
```

### Case 2
``` 
GET api/realmeye/player/guildhistory?name=Cube&prettify
```
Response: 
```json 
{
  "guildHistory": [
    {
      "guildName": "h",
      "guildRank": "Leader",
      "from": "2020-09-21T14:40:27Z",
      "to": ""
    },
    {
      "guildName": "h",
      "guildRank": "Officer",
      "from": "2020-09-20T17:01:26Z",
      "to": "2020-09-21T14:40:27Z"
    },
    // ... omitted
    {
      "guildName": "Shadow Troopers",
      "guildRank": "Initiate",
      "from": "2015-07-29T19:27:25Z",
      "to": "2015-07-29T19:43:45Z"
    },
    {
      "guildName": "Not in a guild",
      "guildRank": "",
      "from": "",
      "to": "2015-07-29T19:27:25Z"
    }
  ],
  "profileIsPrivate": false,
  "sectionIsPrivate": false,
  "name": "cube",
  "resultCode": 200
}
```