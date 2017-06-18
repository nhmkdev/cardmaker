# Contributing

## Community Goals

### Keep It Respectful

This application has been slowly developed for over 6 years. It shows its age in many ways. Please keep this in mind when commenting on the code
or making changes. Unless something needs to change do not risk making frivolous changes because the code and or style is ugly.

The same attitude should apply to all contributors.

### Accepting Changes

I am more than happy to review your work but I cannot promise it will be added to the project or immediately reviewed.

Your changes must compile at a minimum. It is unacceptable to submit code that does not compile to any project. If you cannot compile and test your code but would like a change, please add a feature request instead.

## Software Goals 

### Compatibility

As of this writing compatibility is critical for older projects files. Breaking backward project compatibility will only be considered in extreme cases.

Compatibility is also critical across Windows versions as well as other OS's and environments (virtual machines etc.). For these reasons it is 
important that the application continue to target the basic System.Drawing functionality for rendering.

### Stability

The application should generally be kept as stable as possible. If there are any crashes or extremely broken components a fix
must be made and a new binary released.

## Source Code

### Style

There is no specific style that the code must adhere too. Upon review of the many phases of my own programming style the application maintains
a rough version of the standard C# code style described by Microsoft (by rough I mean really rough).

As of this writing variables are named in a somewhat Hungarian style. While maintaining this is not critical variable names should be clear.

I only recently started to sprinkle `var` usage into the code. Either approach is fine.

Please keep the `{` and `}` characters on their own lines. A few exceptions apply to properties and otherwise but please do not introduce things like
the following:
```
Method(){
}
```

Partial classes are used in a few cases. I like the idea, but I probably abuse them a bit.

There is no specific preference for using `string.format` or `+`.

Updated: 8/21/2015
