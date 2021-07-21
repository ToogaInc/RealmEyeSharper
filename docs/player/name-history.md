[← Go Back](https://github.com/ewang2002/RealmEyeSharper/tree/master/RealmAspNet/docs/docs-guide.md)

# RealmEye API → Player API → Name History
This endpoint gives you the data that is displayed on the player's name history page.

## Endpoint

```
// Gets an unformatted JSON.
GET api/realmeye/player/namehistory?name={name}

// Gets a formatted JSON.
GET api/realmeye/player/namehistory?name={name}&prettify
```

### Arguments

| Argument Name | Type | Required? | Description |
| ---- | ---- | --------- | ----------- |
| `name` | `string` | Yes | The name of the player to request. This is case-insensitive. |
| `prettify` | | No | If included, returns a formatted JSON. |

## TypeScript Definition 
The corresponding TypeScript definition is:
```ts 
interface INameHistory extends IRealmEyePlayerResponse {
    nameHistory: {
        name: string;
        from: string;
        to: string;
    }[];
}
```

## Examples
Here are some examples. 

### Case 1 
```
GET api/realmeye/player/namehistory?name=japan
```
Response:
```json
{"nameHistory":[{"name":"Japan","from":"2017-09-09T09:00:32Z","to":""},{"name":"Japannnnn","from":"2015-01-13T14:11:43Z","to":"2017-09-09T09:00:32Z"},{"name":"Japannnnn","from":"","to":"2015-01-13T14:11:43Z"}],"profileIsPrivate":false,"sectionIsPrivate":false,"name":"japan","resultCode":200}
```

### Case 2 
``` 
GET api/realmeye/player/namehistory?name=meatrod
```
Response: 
```json 
{"nameHistory":[],"profileIsPrivate":false,"sectionIsPrivate":false,"name":"meatrod","resultCode":200}
```

### Case 3 
``` 
GET api/realmeye/player/namehistory?name=Cube&prettify
```
Response: 
```json 
{
  "nameHistory": [
    {
      "name": "Cube",
      "from": "2021-02-16T13:07:31Z",
      "to": ""
    },
    {
      "name": "Cubeee",
      "from": "2018-03-01T16:00:06Z",
      "to": "2021-02-16T13:07:31Z"
    },
    {
      "name": "DrCubeee",
      "from": "",
      "to": "2018-03-01T16:00:06Z"
    }
  ],
  "profileIsPrivate": false,
  "sectionIsPrivate": false,
  "name": "Cube",
  "resultCode": 200
}
```