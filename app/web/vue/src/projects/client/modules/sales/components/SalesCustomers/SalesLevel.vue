<template>
  <div class="d-flex align-items-center">
    <div v-for="(acc, idx) in ibLevelTree" :key="idx">
      <div class="ps-3 fs-4 d-flex align-items-center gap-2">
        <div class="position-relative">
          <div
            class="cursor-pointer text-hover-primary"
            @click="goToIbLevel(acc)"
          >
            {{ acc.nativeName }}
          </div>
          <div class="level-tool-tip">Lv{{ acc.relativeLevel }}</div>
        </div>
        <span class="fw-bold fs-2 text-gray-600">/</span>
      </div>
    </div>

    <button
      @click="resetIbTree"
      class="ms-4 btn btn-sm btn-primary return-btn text-nowrap"
    >
      {{ $t("action.clear") }}
    </button>
  </div>
</template>
<script setup lang="ts">
import { inject } from "vue";

const ibLevelTree = inject<any>("ibLevelTree");
const goToIbLevel = inject<any>("goToIbLevel");
const resetIbTree = inject<any>("resetIbTree");
</script>
<style scoped>
.return-btn {
  padding: 2px 5px !important;
}

.level-tool-tip {
  position: absolute;
  top: -12px;
  right: -18px;
  font-size: 12px;
  height: 17px;
  border-radius: 10px;
  padding: 0 5px;
  background-color: #ffc730;
}
</style>
