<template>
  <tr v-if="isTable">
    <td colspan="100%" class="text-center">
      <div class="data-loading">
        <MoonLoader :color="'#0a46aa'" />
        <p class="mt-3" style="color: gray">{{ $t("tip.loading") }}</p>
      </div>
    </td>
  </tr>
  <div class="data-loading" v-else>
    <MoonLoader :color="'#0a46aa'" />
    <p class="mt-3" style="color: gray">{{ $t("tip.loading") }}</p>
  </div>
</template>

<script setup lang="ts">
import MoonLoader from "vue-spinner/src/PulseLoader.vue";
withDefaults(
  defineProps<{
    // isLoading: boolean;
    width?: number;
    height?: number;
    isTable?: boolean;
  }>(),
  {
    // isLoading: false,
    width: 50,
    height: 50,
    isTable: true,
  }
);
</script>

<style scoped>
.data-loading {
  height: 200px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}
</style>
