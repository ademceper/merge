const uimTheme = require("@merge/uim/tailwind.config");

/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./app/**/*.{js,jsx,ts,tsx}",
    "../../packages/uim/src/**/*.{js,jsx,ts,tsx}",
  ],
  presets: [require("nativewind/preset")],
  theme: {
    extend: {
      ...uimTheme.theme.extend,
      fontFamily: {
        sans: ["SpaceGrotesk-Regular", "system-ui", "sans-serif"],
        light: ["SpaceGrotesk-Light", "system-ui", "sans-serif"],
        medium: ["SpaceGrotesk-Medium", "system-ui", "sans-serif"],
        bold: ["SpaceGrotesk-Bold", "system-ui", "sans-serif"],
      },
    },
  },
  plugins: [],
}

