# Jamb

Simple multiplayer Yahtzee game implemented in .NET.

## What I want to do

Implement a game that can only be played by 2 players over the network.

It should be able to:
* Gracefully handle all possible failures.
* Support recovery in a sense that the game can be saved and resumed.
* User interface should be intuitive and appealing. In essence it should communicate everything that is inherently available when playing the game in-person.
* It should support exchanging basic text messages within a game.
* Aspects of the game that are fixed in the implementation should support extension with reasonable amount of work. For example number of columns or dices.
* Protect itself from malicious attacks from the network.

It is not aiming to:
* Support multiple versions of communication protocol and/or application. Both players must have same version of the application running.
* Secure the communication channel. Connection will not be encrypted and other people are able to intercept the traffic and/or execute man in the middle attack.

## Architecture

Architecture is yet to be described here.

## Coding principles

1. [SOLID](https://en.wikipedia.org/wiki/SOLID_(object-oriented_design)) principles
2. Do [Encapsulate](https://en.wikipedia.org/wiki/Encapsulation_(computer_programming))
3. Code should be packaged in a way it can be easily used in other projects. [See](https://en.wikipedia.org/wiki/Package_principles)
4. Don't write the same code twice. [See](https://en.wikipedia.org/wiki/Don%27t_repeat_yourself)

## Coding convention

1. All code must be covered with unit tests as reasonably as possible.
2. Integration tests should be present for each external dependency that can't be reasonably covered in unit tests.
3. Each method that is visible outside of assembly must:
   * Test its arguments in production code and
   * Have complete documentation (e.g. using <summary>).
4. Methods that are not visible outside of assembly and are not private to the class should:
   * Test their arguments in production code unless they explicitly specify that "clean" arguments are expected and
   * Have a documentation unless there is nothing to communicate. There is no point in adding <summary>Default constructor</summary> above a default constructor, that information is already presented and clearly communicated through code.
5. Methods that are private to the class should:
   * Test their arguments using Debug.Assert unless otherwise specified and
   * Have documentation unless there is nothing to communicate.
6. All code should be commented to the reasonable extent. Comments should state what and why rather than how. Comments can be omitted if code is self documenting, for example through well named functions/variables etc.
7. Avoid stating the obvious in comments e.g. "Default constructor".
8. Constructor should never fail unless input argument is not within expected bounds. That is constructor should validate arguments and avoid doing anything that could fail.

Remaining is TBD. Will use same style throughout project so it should be easy to mimic the existing style used.

## If you want to contribute

Given that current architecture and development plan is not yet written it is best to email me at: vileamail [at] gmail [dot] com