// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  devtools: { enabled: false },
  
  css: ['~/assets/css/main.css'],

  modules: [
    '@nuxtjs/tailwindcss',
    '@nuxtjs/google-fonts'
  ],

  postcss: {
    plugins: {
      tailwindcss: {},
      autoprefixer: {},
    },
  },

  googleFonts: {
    display: 'block',

    families: {
      Montserrat: {
        wght: [100, 200, 300, 400, 500, 600, 700, 800, 900]
      }
    },

    prefetch: false,
    preload: false,

    download: false,
    base64: false,
    inject: false,
    overwriting: false,
    outputDir: 'assets/fonts',
    stylePath: 'css/google-fonts.css',
    fontsPath: '../'
  }
})
