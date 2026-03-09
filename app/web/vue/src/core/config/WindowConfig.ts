import { computed, ref } from "vue";

const windowWidth = ref(window.innerWidth);
// window.addEventListener("resize", updateWidth);
// window.removeEventListener("resize", updateWidth);
// the listener should be added to ClientApp.vue
export const updateWidth = () => {
  windowWidth.value = window.innerWidth;
};

export const isMobile = computed(() => windowWidth.value < 768);

export const isTablet = computed(
  () => windowWidth.value < 1024 && windowWidth.value >= 768
);

export const isDesktop = computed(() => windowWidth.value >= 1024);
