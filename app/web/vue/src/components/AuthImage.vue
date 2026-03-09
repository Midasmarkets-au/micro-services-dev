<template>
  <img
    :src="src"
    :style="{
      width: size,
      height: size,
    }"
    alt=""
  />
</template>
<script lang="ts" setup>
import { computed, onMounted, ref, watch } from "vue";
import JwtService from "@/core/services/JwtService";
import blankImage from "@/assets/media/folder/blank-image.svg";

const props = withDefaults(
  defineProps<{
    imageGuid: string;
    side?: string;
    size?: string;
  }>(),
  {
    imageGuid: "",
    side: "client",
    size: "40px",
  }
);

const urlValid = ref(true);

const processUrl = async () => {
  if (props.imageGuid === "-1") return blankImage;
  let img_src = "";
  // console.log("props.url: ", props.url);
  if (props.imageGuid === "no-image") img_src = "/images/no-image.png";
  else if (
    props.imageGuid === null ||
    props.imageGuid === "" ||
    props.imageGuid === "no-avatar"
  ) {
    img_src = "/images/avatar.png";
  } else if (/^https:\/\//.test(props.imageGuid)) {
    img_src = props.imageGuid;
  } else {
    img_src =
      window["api"] +
      "/api/v1/" +
      (props.side ? (props.side as string) : "tenant") +
      "/media/" +
      props.imageGuid +
      "?access_token=" +
      JwtService.getToken();
  }
  urlValid.value = await isImageSrcValid(img_src);
  return urlValid.value ? img_src : blankImage;
};

const src = ref("");
watch(
  () => props.imageGuid,
  async () => {
    src.value = await processUrl();
  }
);

onMounted(async () => {
  src.value = await processUrl();
});

function isImageSrcValid(src: string): Promise<boolean> {
  return new Promise((resolve) => {
    const img = new Image();
    img.src = src;

    img.onload = () => {
      resolve(true); // Valid image URL
    };

    img.onerror = () => {
      resolve(false); // Invalid image URL
    };
  });
}
</script>
