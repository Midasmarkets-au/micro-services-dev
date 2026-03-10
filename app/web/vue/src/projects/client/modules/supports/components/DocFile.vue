<template>
  <div class="col-12 col-sm-12 col-xl">
    <div class="card h-100">
      <div v-if="isMobile" class="card-body d-flex align-items-center">
        <img
          :src="`/images/files/${props.fileType}.png`"
          alt=""
          style="width: 40px; height: 40px"
        />
        <a
          :href="props.fileLink"
          target="_blank"
          class="fs-5 fw-semibold ms-5 text-black text-opacity-75 cursor-pointer text-hover-primary"
        >
          {{ props.fileTitle }}
        </a>
      </div>

      <div
        v-if="!isMobile"
        class="card-body d-flex align-items-center flex-row p-8 align-items-stretch"
      >
        <div class="text-gray-800 d-flex flex-column">
          <div class="symbol symbol-60px mb-6">
            <img :src="`/images/files/${props.fileType}.png`" alt="" />
          </div>
        </div>
        <div class="ml-6">
          <div class="fs-5 fw-semibold mb-2">
            {{ props.fileTitle }}
          </div>
          <a
            v-bind:href="props.fileLink"
            target="_blank"
            class="btn btn-light-primary btn-xs btn-bordered"
          >
            {{ props.createdAt }}
          </a>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { isMobile } from "@/core/config/WindowConfig";

const props = defineProps<{
  fileTitle: string;
  createdAt: string;
  fileType: string;
  fileLink: string;
}>();
</script>

<style>
.download-btn:hover {
  cursor: pointer;
}
</style>
