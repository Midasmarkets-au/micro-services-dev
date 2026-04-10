<template>
  <div
    style="position: relative"
    class="menu-gray-600"
    @mouseover="showDropdownLang = true"
    @mouseleave="showDropdownLang = false"
  >
    <span class="current-language flex">
      <img
        class="w-25px h-25px rounded-1 ms-2"
        :src="currentLanguageLocale.flag"
        alt="MDM Pro"
        style="margin-right: 8px"
      />
      {{ currentLanguageLocale.name }}
    </span>
    <div style="height: 20px"></div>
    <div
      v-if="showDropdownLang"
      @mouseleave="showDropdownLang = false"
      class="menu-dropdown"
    >
      <div
        class="menu-item px-3 language-item"
        v-for="(lang, index) in countries"
        :key="index"
      >
        <a
          @click="setLang(lang.code)"
          href="#"
          class="menu-link d-flex px-1"
          :class="{ active: currentLanguage(lang.code) }"
        >
          <span class="symbol symbol-20px me-4">
            <img class="rounded-1" :src="lang.flag" alt="MDM Pro" />
          </span>
          {{ lang.name ?? "Name" }}
        </a>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from "vue";
import { useI18n } from "vue-i18n";
import { ILanguage, LanguageTypes } from "@/core/types/LanguageTypes";
import { getTenantLanguagesOptions } from "@/core/types/TenantTypes";

const i18n = useI18n();
let countries: ILanguage[];
countries = getTenantLanguagesOptions.value;
const showDropdownLang = ref(false);

// eslint-disable-next-line @typescript-eslint/no-empty-function
onMounted(async () => {});

const setLang = (language: string) => {
  showDropdownLang.value = !showDropdownLang.value;
  i18n.locale.value = language;
  localStorage.setItem("language", language);
};

const currentLanguage = (language: string) => {
  return i18n.locale.value === language;
};

const currentLanguageLocale = computed(() => {
  return (
    countries.find((x) => x.code === i18n.locale.value) ?? LanguageTypes.enUS
  );
});
</script>

<style lang="scss" scoped>
.menu-link {
  background-color: white;
  border-radius: 10px;
}
.menu-link:hover {
  background-color: var(--kt-primary-light);
  color: var(--kt-primary) !important;
}

.menu-dropdown {
  position: absolute;
  background-color: white;
  border-radius: 10px;
  // top: 40px;
  border: 1px solid #e4e6ef;
  z-index: 1;
  width: max-content;

  @media (max-width: 768px) {
    // width: 200px;
    right: -15px;
  }
}
</style>
