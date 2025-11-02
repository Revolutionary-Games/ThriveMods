# ThriveMods

Official and example Thrive mods

We don't currently accept community mods into this repo, you need to put them elsewhere.

## Example Mods

- [Disco Nucleus](DiscoNucleus): shows how to replace an existing asset in Thrive
- [Damage Numbers](DamageNumbers): example mod for using C# in a mod
- [Random Part Challenge](RandomPartChallenge): example mod for using Harmony manually in a C#
- [Cell Autopilot](CellAutopilot): example mod for using Harmony in a C# standalone mod not using Godot at all

Note that the top level solution (`.sln`) file doesn't reference the code mods as they don't
want to compile that way.

## Exporting

For the C# mods to work, this repository needs to be cloned next to
the Thrive repository for code references to work. Also Thrive needs
to have been exported at least once to generate the library files that
are referenced.

There's a helper script provided (`dotnet run --project Scripts -- package --clean-zip`)
that exports and prepares folders for all mods in this repository. For how to get the
script working, please refer to [Thrive setup
instructions](https://github.com/Revolutionary-Games/Thrive/blob/master/doc/setup_instructions.md)
If you prefer you can also manually use Godot editor to export
specific mods. Some mods are C# only and just need C# build tools
without depending on Godot.


## Referencing Thrive

The code mods use a relative path to refer to Thrive. If the Godot
engine ever changes how their build output is structured this also
needs changing. The base path is:
`.godot\mono\temp\bin\Debug\Thrive.dll`

Note: Windows path separator used as that's what's in `.csproj` files.
