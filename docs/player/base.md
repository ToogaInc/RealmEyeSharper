[← Go Back](https://github.com/ewang2002/RealmEyeSharper/tree/master/RealmAspNet/docs/docs-guide.md)

# RealmEye API → Player API → Base 
This represents the base of every request made to the API. When you send an API request to one of the player endpoints, you are guaranteed to get the following properties:
```json
{
  "profileIsPrivate": boolean,
  "sectionIsPrivate": boolean,
  "resultCode": number, 
  "name": string
}
```

The corresponding TypeScript interface is:
```ts 
interface IRealmEyePlayerResponse {
    profileIsPrivate: boolean;
    sectionIsPrivate: boolean;
    resultCode: number;
    name: string;
}
```

## Property Definitions
- `profileIsPrivate`: Whether the profile is private. This is `true` if the person's profile could not be found and `false` if the person's profile could be found. 
- `sectionIsPrivate`: Whether the specific section is private. This is `true` if the specific data you're looking for (e.g. name history, pet yard, etc.) is private and `false` otherwise.
- `name`: The requested name. If no name was given, this will be an empty string. 
- `resultCode`: The resultant code. This can either be `200` (Success), `400` (NotFound), `500` (InternalServerError), or `503` (ServiceUnavailable). 