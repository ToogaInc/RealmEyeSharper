# RealmSharper & RealmAspNet
An ASP.NET API Application that is designed to scrape RealmEye and provide some additional support for Realm of the Mad God (RotMG) Discord raiding bots and applications.

The `RealmSharper` solution contains the actual scraper logic. That is, `RealmSharper` contains code that is designed to scrape various aspects of a user's RealmEye profile.

The `RealmAspNet` solution, as the name implies, is the ASP.NET Core application that any other codebase can make requests to. This solution also contains some extra features like a basic `/who` parser.

## Built With
- .NET 5.0
- ASP.NET Core

## Getting Started
Coming Soon.

## Basic Endpoints Guide
Here are all of the endpoints that this application supports.

### RealmEye Player Profile
Here, `{name}` represents the name of the player that you want to request data for. Generally speaking, the information that you get from a particular endpoint and from the corresponding RealmEye page will not differ by much; in other words, you should expect to get most of the information that you would expect from RealmEye in a JSON format.

| API Endpoint | Description |
| ------------ | ----------- |
| `GET api/realmeye/player/basics/{name}` | Gets basic player data.  |
| `GET api/realmeye/player/petyard/{name}` | Gets the person's pet yard. |
| `GET api/realmeye/player/graveyard/{name}/{amt?}` | Gets the person's graveyard information. Will get up to the 100 most recent entries. | 
| `GET api/realmeye/player/graveyardsummary/{name}` | Gets the person's graveyard summary. | 
| `GET api/realmeye/player/namehistory/{name}` | Gets the person's name history. | 
| `GET api/realmeye/player/famehistory/{name}` | Gets the person's fame history. Not working at this time. |
| `GET api/realmeye/player/guildhistory/{name}` | Gets the person's guild history. | 
| `GET api/realmeye/player/rankhistory/{name}` | Gets the person's rank history. |
| `GET api/realmeye/player/exaltations/{name}` | Gets the person's exaltations. |

### Parse Who Endpoint
This endpoint will parse a __cropped__ `/who` screenshot. It will do some basic image processing before actually parsing it, though. 

Endpoint: `GET api/raidutil/parsewho`

You must include the following in your body when making a request to this endpoint:
```js
{
    Url: "link/to/image/with/cropped/who"
}
```

This will return a JSON that looks like:
```
{
    "imageDownloadTime": number,
    "imageProcessingTime": number,
    "ocrRecognitionTime": number,
    "whoResult": string[],
    "rawOcrResult": string,
    "count": number,
    "code": string,
    "issues": string
}
```

### Online Test
Use this endpoint to test if the API is online.

Endpoint: `api/online`

This is guaranteed to return a JSON that looks like:
```
{
    "online": true
}
```

If it wasn't clear enough, the idea is that if the API is online, then you would get a response. Otherwise, your HTTP client will throw some sort of an exception. In particular, this endpoint is useful if you want to check if the API is online *before* making any further requests.

## Acknowledgements
Inspired by Nightfirecat's [RealmEye API](https://github.com/Nightfirecat/RealmEye-API).

## License
MIT License.