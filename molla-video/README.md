# Molla animated demo

A silent, editable 12-second animation using Molla's actual apple UI and brand
icon. Square 1080 × 1080, 30 fps, composition `MollaDemo`.

## Molla preview

From this directory, run:

```sh
npm install --include=dev
npm run studio
```

Open the localhost URL printed by Studio and select `MollaDemo`.
Use Play or scrub the timeline. `npm run check` checks TypeScript.

## Storyboard

- 0–4 seconds: introduce Molla and drag a PNG onto JPG.
- 4–7 seconds: show conversion progress.
- 7–9 seconds: reveal the JPG export, with the original kept.
- 9–12 seconds: close on the icon and “Your files. A fresh form.”

This is an animated demonstration, not a screen recording or a live converter.
It depicts a supported PNG-to-JPG conversion; Molla does not convert every file type.

Edit `src/Root.tsx` for layout, copy, and timing. Assets are bundled in `public/`.
When ready, use Studio's Render button to export `MollaDemo`.
The editable preview is supplied; no final MP4 is included yet.

## Remotion resources

<p align="center">
  <a href="https://github.com/remotion-dev/logo">
    <picture>
      <source media="(prefers-color-scheme: dark)" srcset="https://github.com/remotion-dev/logo/raw/main/animated-logo-banner-dark.apng">
      <img alt="Animated Remotion Logo" src="https://github.com/remotion-dev/logo/raw/main/animated-logo-banner-light.gif">
    </picture>
  </a>
</p>

Welcome to your Remotion project!

## Commands

**Install Dependencies**

```console
npm i --loglevel=error
```

**Start Preview**

```console
npm run studio
```

**Render video**

```console
npx remotion render
```

**Upgrade Remotion**

```console
npx remotion upgrade
```

## Docs

Get started with Remotion by reading the [fundamentals page](https://www.remotion.dev/docs/the-fundamentals).

## Help

We provide help on our [Discord server](https://discord.gg/6VzzNDwUwV).

## Issues

Found an issue with Remotion? [File an issue here](https://github.com/remotion-dev/remotion/issues/new).

## License

Note that for some entities a company license is needed. [Read the terms here](https://github.com/remotion-dev/remotion/blob/main/LICENSE.md).
