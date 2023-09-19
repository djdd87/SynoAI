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
      Montserrat: true
    },

    prefetch: false,
    preload: false,

    download: true,
    base64: false,
    inject: true,
    overwriting: false,
    outputDir: 'assets/fonts',
    stylePath: 'css/google-fonts.css',
    fontsPath: '../'
  }
})
