[← Go Back](https://github.com/ewang2002/RealmEyeSharper/tree/master/RealmAspNet/docs/docs-guide.md)

# RealmEye API → Player API → Pet Yard
This endpoint gives you the data that is displayed on the player's pet yard page.

## Endpoint
```
// Gets an unformatted JSON.
GET api/realmeye/player/petyard?name={name}

// Gets a formatted JSON.
GET api/realmeye/player/petyard?name={name}&prettify
```

## TypeScript Definition 
The corresponding TypeScript definition is:
```ts 
interface IPetYard extends IRealmEyePlayerResponse {
    pets: {
        id: number;
        petSkinName: string;
        name: string;
        rarity: string;
        family: string;
        place: number;
        petAbilities: {
            isUnlocked: boolean;
            abilityName: string;
            level: number;
            isMaxed: boolean;
        }[];
        maxLevel: number;
    }[];
}
```

### Arguments

| Argument Name | Type | Required? | Description |
| ---- | ---- | --------- | ----------- |
| `name` | `string` | Yes | The name of the player to request. This is case-insensitive. |
| `prettify` | | No | If included, returns a formatted JSON. |

## Examples
Here are some examples. 

### Case 1 
```
GET api/realmeye/player/petyard?name=kbgawugbr
```
Response:
```json
{"profileIsPrivate":true,"sectionIsPrivate":true,"name":"kbgawugbr","resultCode":400}
```

### Case 2 
``` 
GET api/realmeye/player/petyard?name=consolemc&prettify
```
Response: 
```json
{
  "pets": [
    {
      "id": 30126,
      "petSkinName": "Infested Bagston",
      "name": "Infested Bagston",
      "rarity": "Legendary",
      "family": "? ? ? ?",
      "place": 5674,
      "petAbilities": [
        {
          "isUnlocked": true,
          "abilityName": "Heal",
          "level": 90,
          "isMaxed": true
        },
        {
          "isUnlocked": true,
          "abilityName": "Electric",
          "level": 89,
          "isMaxed": false
        },
        {
          "isUnlocked": true,
          "abilityName": "Magic Heal",
          "level": 78,
          "isMaxed": false
        }
      ],
      "maxLevel": 90
    },
    // ... omitted
    {
      "id": 32533,
      "petSkinName": "Bluebird",
      "name": "Bluebird",
      "rarity": "Uncommon",
      "family": "Avian",
      "place": -1,
      "petAbilities": [
        {
          "isUnlocked": true,
          "abilityName": "Heal",
          "level": 50,
          "isMaxed": true
        },
        {
          "isUnlocked": true,
          "abilityName": "Savage",
          "level": 42,
          "isMaxed": false
        },
        {
          "isUnlocked": false,
          "abilityName": "Decoy",
          "level": -1,
          "isMaxed": false
        }
      ],
      "maxLevel": 50
    }
  ],
  "profileIsPrivate": false,
  "sectionIsPrivate": false,
  "name": "consolemc",
  "resultCode": 200
}
```