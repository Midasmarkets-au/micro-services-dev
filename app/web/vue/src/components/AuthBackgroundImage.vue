<template>
  <div :style="`background-image: url(${src});`"></div>
</template>
<script lang="ts" setup>
import { ref, watch } from "vue";
import JwtService from "@/core/services/JwtService";

// const props = defineProps({
//   url: { type: String, required: true },
// });

const props = defineProps<{
  fileKey: string;
}>();

const src = ref("");

const processImageUrl = (key: string) => {
  let img_src = "";
  if (key === "no-image") img_src = "/images/no-image.png";
  else if (key === null || key === "" || key === "no-avatar") {
    img_src = "/images/avatar.png";
  } else {
    img_src =
      window["api"] +
      "/api/v1/tenant/media/" +
      key +
      "?access_token=" +
      JwtService.getToken();
  }
  return img_src;
};

src.value = processImageUrl(props.fileKey);

watch(
  () => props.fileKey,
  function () {
    src.value = processImageUrl(props.fileKey);
    console.log("src.value", src.value);
  }
);
</script>
