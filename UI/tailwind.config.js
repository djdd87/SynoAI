/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./components/**/*.{js,vue,ts}",
    "./layouts/**/*.vue",
    "./pages/**/*.vue",
    "./plugins/**/*.{js,ts}",
    "./nuxt.config.{js,ts}",
    "./app.vue",
    "./assets/**/*.{css}",
  ],
  plugins: [],
  darkMode: 'media', // or 'false' or 'true' or 'class'
  theme: {
  },
}

