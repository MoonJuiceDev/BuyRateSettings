## 1.3.0
- Removed terminal command (it didn't sync with host properly and had issues with changing save files or reloading the game. I'm shifting focus to other mods so I don't have the time or interest in fixing this lol)
    - DELETE YOUR OLD CONFIG FILE TO REMOVE THE TERMINAL STUFF
- Added last day rate chance - the chance that the last day rate will be within the range you have set instead of the vanilla 100%
- Logging now shows unrounded rates as well
- Open sourced the mod

## 1.2.1
- New icon :)

## 1.2.0
- Added new terminal command `refresh rate` that shows in the `Other` menu (EDIT: only works properly for host and has bugs)
- Added config options to enable/disable the terminal command
- Added config options to limit the terminal command's usage per day
- Lots of code cleanup and restructuring
- Massive thank you to my brother [Cryptoc1](https://thunderstore.io/c/lethal-company/p/Cryptoc1/) for holding my hand

## 1.1.2
- Added a hidden, delayed rate setter - Sets the companyBuyRate variable a 2nd time after a 3 second delay to avoid the rate being overwritten by other mods at the start of a round or new quota. (NOTE: If you set the Alert Delay to less than 3 seconds, it may show the wrong rate in the alert. Just don't do that, that'd be weird.)

## 1.1.1
- Added Alert Delay setting for compatibility with other mods that use the same alerts

## 1.1.0
- Added in-game alerts for rates/jackpots
- Added Jackpot rate
- Added some logging
- New icon
- Updated config, readme, changelog

## 1.0.1
- Added min/max range for last day rate
- Updated config & readme (DELETE OLD CONFIG)

## 1.0.0
- Initial release