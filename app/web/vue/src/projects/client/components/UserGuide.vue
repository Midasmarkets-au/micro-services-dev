<template>
  <div class="card card-flush mb-2 mt-1" v-if="show">
    <div class="card-header">
      <div class="card-title">
        <h3 class="d-flex align-items-center mx-1 m-0 fw-bold">
          {{ $t("title.userGuide") }}
        </h3>
      </div>
    </div>
    <div class="card-body pt-0 pb-2 mt-1">
      <div class="fs-6 d-flex gap-3">
        <div
          class="guide-box"
          :class="{ active: activeIndex === 0 }"
          @click="openVideo(0)"
        >
          <div class="d-flex align-items-center gap-1">
            <el-icon><Film /></el-icon>
            <span>{{ $t("title.myBcrGuide") }}</span>
          </div>
          <!-- <p class="cursor-pointer text-hover-primary" @click="openVideo(0)">
            {{ $t("action.view") }}
          </p> -->
        </div>
        <div
          v-if="$can('IB')"
          :class="{ active: activeIndex === 1 }"
          class="guide-box"
          @click="openVideo(1)"
        >
          <div class="d-flex align-items-center gap-1">
            <el-icon><Film /></el-icon> <span>{{ $t("title.ibGuide") }}</span>
          </div>
          <!-- <p class="cursor-pointer text-hover-primary" @click="openVideo(1)">
            {{ $t("action.view") }}
          </p> -->
        </div>
      </div>
    </div>
  </div>
  <VideoModal ref="videoRef" @close="handleVideoClose" />
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import VideoModal from "./VideoModal.vue";
import { userGuideList } from "@/core/data/userGuide";
import { Film } from "@element-plus/icons-vue";
import { getLanguage } from "@/core/types/LanguageTypes";
import { tenancies, getTenancy } from "@/core/types/TenantTypes";
const activeIndex = ref<number | null>(null);
var language = getLanguage.value;
const show = ref(true);

if (language == "zh-hk") {
  language = "zh-tw";
} else if (language == "id-id") {
  language = "en-us";
}

const handleVideoClose = () => {
  activeIndex.value = null;
};
const videoRef = ref<any>(null);

const openVideo = (id: number) => {
  activeIndex.value = id;
  const guide = userGuideList[id];
  videoRef.value?.show(guide.src);
};

onMounted(() => {
  if (getTenancy.value !== tenancies.jp) {
    show.value = true;
  } else {
    show.value = false;
  }
});
</script>
<style scoped>
.guide-box {
  height: 5.285rem;
  background-color: #fff;
  box-shadow: 0 8px 10px rgba(63, 119, 212, 0.3);
  border: 1px solid #fff;
  align-items: center;
  border-radius: 8px;
  padding: 1rem;
  flex: 1;
  display: flex;
  margin-bottom: 1.43rem;
  cursor: pointer;
  &:hover,
  &.active {
    background-color: #000f32;
    color: #fff;
    a {
      color: #fff;
      div {
        color: #fff;
      }

      svg path {
        fill: #fff;
      }
    }
  }
  &:link {
    background-color: #000f32;
  }
  @media (max-width: 1512px) {
    font-size: 13px;
  }
}
</style>
