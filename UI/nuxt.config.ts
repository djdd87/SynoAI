// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  css: ['~/assets/css/main.css'],
  devtools: { enabled: true },
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
    display: 'swap',

    families: {
      Montserrat: true
    },

    prefetch: false,
    preload: true,

    download: true,
    base64: false,
    inject: false,
    overwriting: false,
    outputDir: '~/assets/fonts'
  }
})
