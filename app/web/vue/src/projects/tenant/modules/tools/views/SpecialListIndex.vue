<template>
  <div>
    <ul
      class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
    >
      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="[
            { active: tab === 'cheaterIp' },
            { 'disabled opacity-50 pe-none': isLoading },
          ]"
          data-bs-toggle="tab"
          href="#"
          @click="tab = 'cheaterIp'"
          >{{ $t("title.cheaterIp") }}</a
        >
      </li>

      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="[
            { active: tab === 'blockedList' },
            { 'disabled opacity-50 pe-none': isLoading },
          ]"
          data-bs-toggle="tab"
          href="#"
          @click="tab = 'blockedList'"
          >{{ $t("title.blockedList") }}</a
        >
      </li>
      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="[
            { active: tab === 'violationList' },
            { 'disabled opacity-50 pe-none': isLoading },
          ]"
          active
          data-bs-toggle="tab"
          href="#"
          @click="tab = 'violationList'"
          >{{ $t("title.violationList") }}</a
        >
      </li>
    </ul>
    <CheaterIp v-if="tab === 'cheaterIp'" />
    <BlockedList v-if="tab === 'blockedList'" />
    <ViolationList v-if="tab === 'violationList'" />
  </div>
</template>

<script setup lang="ts">
import CheaterIp from "@/projects/tenant/modules/tools/modals/CheaterIp.vue";
import BlockedList from "@/projects/tenant/modules/tools/modals/BlockedList.vue";
import ViolationList from "@/projects/tenant/modules/tools/modals/ViolationList.vue";
import { ref, provide } from "vue";
const tab = ref("cheaterIp");
const isLoading = ref(false);
provide("isLoading", isLoading);
</script>
