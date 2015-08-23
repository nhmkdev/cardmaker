# CardMaker

An application that generates graphics via data sources. (Coming soon: Samples / Sample images)

## Download

Latest binary: [Download 0.99.1.10](https://www.nhmk.com/applications/CardMaker_0.99.1.10.zip)

* [Tutorials](https://www.nhmk.com/cardmakertutorials/)
* [CardMaker User Guides Section](https://github.com/nhmkdev/cardmaker/wiki/user) (Work in progress, all tutorials will be moved over)

Note: The code that is submitted may not match the latest binary.

## Code Status

Compiles with Visual Studio Express Desktop Edition / Visual Studio 2012

Toxicity Level: Medium-High
 * Needs more comments
 * Needs warning cleanup
 * Needs some shuffling to allow for more narrow and focused unit testing
 * Needs more unit tests!
 * Every method should be implemented based on an interface and classes should implement hundreds of interfaces (just kidding!!!)

## The Google Issue

If you plan to outright copy the parts of the code that operate with Google Spreadsheets please be aware that the source
currently uses the client id that is associated with my CardMaker application. [Code File](https://raw.githubusercontent.com/nhmkdev/cardmaker/master/CardMaker/Card/Import/GoogleReferenceReader.cs)

You will need to modify this code to correctly use your application.

## History

2009 - First created and publicly released CardMaker

20XX - Lots of bug fixes and features

2015 - CardMaker goes open source

### The Name

"CardMaker" and "Card Maker" are both references to the same application. I just never standardized the name.

## Special Thanks

* Eric Raue
* Kolja Geldmacher
* Everyone that has contributed to the project through emailing bugs and requesting features

## Original Author

CardMaker was originally created in 2009 by Tim Stair.
