# Docs

> [!NOTE]
> Make sure to read the [environment setup](../environment-setup.md) guide before continuing.

The docs are built using [docfx](https://dotnet.github.io/docfx/). To get started:

```shell
dotnet tool restore
cd docs
dotnet docfx .\docfx.json --serve
```

The docs will then be available at <http://localhost:8080>.

To build the docs while the server is already running, run in a new terminal:

```shell
dotnet docfx .\docfx.json
```

## Linking

When linking to the docs for one of Whim's automatically generated docs, prefer using the `xref` [Markdown autolinks](https://dotnet.github.io/docfx/docs/links-and-cross-references.html?q=xref#markdown-autolink) or the `xref` [Markdown links](https://dotnet.github.io/docfx/docs/links-and-cross-references.html#markdown-link).

The IDs to use can be found in the `uid` field of the `xrefmap.yml` file. The `xrefmap.yml` will be generated at `docs/_site/xrefmap.yml`.

## Things to keep in mind

- Use American English spelling for the docs.
- The table of contents are not automatically built - to add a new page, you need to add it to the respective `toc.yml`.
- The recommended extensions for Visual Studio Code include a variety of useful extensions, including [markdownlint](https://github.com/markdownlint/markdownlint).
