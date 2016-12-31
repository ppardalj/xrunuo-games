# Automatic games for XRunUO

This is a script collection for [XRunUO](https://github.com/xrunuo/xrunuo) that allows you to set up and develop automatic games, such as Color War or Capture the Flag, in your Ultima Online freeshard.

The bundle includes:
* A framework to develop automatic games.
* Four fully functional games: Capture the flag (CTF), Color war (CW), Total war and Survival.

## Installation

1. Create a folder `Games` inside your XRunUO `Scripts` folder.
2. Drop the contents of the `Scripts` folder of this repository into your XRunUO `Scripts/Games/` folder.
3. Open your `x-runuo.xml` config file, and add the library `Games` in the `libraries` section.
4. Turn on your XRunUO emulator and you're all set!

## Configuration

Once in the game, as an administrator, run the command `.GameManager` to see the main game managing UI. You can add new games, delete existing games, edit their properties, etc.

Some configuration parameters are hardcoded in the source code. If you want to change them, you will have to go through the code to find and change exactly what you want.

## Developing new games

At the moment there is no full guide on how you can develop new games, but you can dig into the four provided games to get an insight on how to do it yourself. Also you can contact project maintainer if you need further support. Contributions are welcomed (see "Contributing" section).

## Contributing

If you encounter an issue feel free to submit a pull request. You can also go through the project issue list, solve it yourself and send a pull request afterwards.

Feel free to submit documentation, guides or feature requests in the project issue list.

## Disclaimer

The current code was initially developed for the former UO free shard _UO Legends_. The code is provided as it was when the server was closed and the development stopped. Some features may be unstable, and some code may not fit generic needs.

As there is no official release yet, the public interface of the game developing framework is considered unstable.

## License

Copyright (C) 2017 Pedro Pardal

This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <http://www.gnu.org/licenses/>.
