# RealmEyeSharper
An unofficial RealmEye API.

## Formal Description

An ASP.NET API Application that is designed to scrape RealmEye and provide some additional support for Realm of the Mad God (RotMG) Discord raiding bots and applications.

## Built With
- .NET 5.0
- ASP.NET

## Getting Started
Coming Soon.

## Basic Endpoints Guide
Here are all of the endpoints that this application supports.

### RealmEye Player Profile
Here, `{name}` represents the name of the player that you want to request data for. Generally speaking, the information that you get from a particular endpoint and from the corresponding RealmEye page will not differ by much; in other words, you should expect to get most of the information that you would expect from RealmEye in a JSON format.

| API Endpoint | Description |
| ------------ | ----------- |
| `GET api/realmeye/player/basics?name={name}` | Gets basic player data.  |
| `GET api/realmeye/player/petyard?name={name}` | Gets the person's pet yard. |
| `GET api/realmeye/player/graveyard?name={name}/{amt?}` | Gets the person's graveyard information. Will get up to the 100 most recent entries. | 
| `GET api/realmeye/player/graveyardsummary?name={name}` | Gets the person's graveyard summary. | 
| `GET api/realmeye/player/namehistory?name={name}` | Gets the person's name history. |
| `GET api/realmeye/player/guildhistory?name={name}` | Gets the person's guild history. | 
| `GET api/realmeye/player/rankhistory?name={name}` | Gets the person's rank history. |
| `GET api/realmeye/player/exaltations?name={name}` | Gets the person's exaltations. |

### Get Multiple RealmEye Profiles Endpoint
This endpoint is exactly the same as calling the `basic` endpoint, but this endpoint will accept an array of names.

Endpoint: `POST api/raidutil/parseWho`

In your request body, only include the array of names. 

This endpoint requires the use of rotating proxies (since RealmEye has a rate limit). This API supports the use of [Webshare.io](https://webshare.io/), which is a **paid** service (the free service is insufficient due to RealmEye blocking proxies). 

### Parse Who Endpoint
This endpoint will parse a __cropped__ `/who` screenshot.

Endpoint: `POST api/raidutil/parseNamesForREProfiles`

You must include the following in your body when making a request to this endpoint:
```
{
    "Url": string,
    "GetRealmEyeData": bool?
}
```
Here, `URL` is the URL to the image. This endpoint will return two things depending on the value of `GetRealmEyeData`.
- If `GetRealmEyeData` is false or isn't included, then this endpoint will return an array of names that were parsed. 
- Otherwise, this endpoint will return a JSON containing information like the RealmEye profiles corresponding to each name found in the screenshot.

The OCR parsing requires an [OCRSpace](https://ocr.space/) API key, which is free.

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