<template>
  <div
    :class="{
      'rounded-circle overflow-hidden': rounded,
    }"
    :style="{
      width: size,
      height: size,
    }"
  >
    <div class="symbol avatar-bg" :class="avatarClass" v-if="src !== ''">
      <AuthImage :imageGuid="avatar" :side="side" :size="size" />
    </div>
    <div class="symbol" :class="avatarClass" v-else>
      <div
        class="symbol-label fs-3 bg-light-danger"
        :class="{ letterAvatarColor: true }"
        :style="{
          width: size + '!important',
          height: size + '!important',
        }"
      >
        {{ avatarLetter }}
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
import { ref } from "vue";
import AuthImage from "@/components/AuthImage.vue";
import { isMobile } from "@/core/config/WindowConfig";

const props = defineProps({
  avatar: { type: String },
  name: { type: String, required: true },
  size: { type: String, default: "40px" },
  side: { type: String, default: "tenant" },
  rounded: { type: Boolean, default: false },
});
const src = ref("");
const avatarLetter = ref("");
const avatarClass = ref("symbol-" + props.size);
const letterAvatarColor = ref("");
// [
//   "bg-light-primary",
//   "bg-light-danger",
//   "bg-light-success",
//   "bg-light-warning",
//   "bg-light-info",
//   "bg-light-dark",
// ];

const processImageUrl = (url: string) => {
  let img_src = "";
  if (url == "no-image") {
    img_src = "/images/no-image.png";
  } else if (url == "no-avatar" || url == "") {
    img_src = "";
    let tname = props.name;
    if (tname == undefined || tname == "") {
      tname = "No Name";
    }

    avatarLetter.value = tname.charAt(0).toUpperCase();
    if (["A", "B", "C", "D", "E"].includes(avatarLetter.value)) {
      letterAvatarColor.value = "text-primary";
    } else if (["F", "G", "H", "I", "J"].includes(avatarLetter.value)) {
      letterAvatarColor.value = "text-danger";
    } else if (["K", "L", "M", "N", "O"].includes(avatarLetter.value)) {
      letterAvatarColor.value = "text-success";
    } else if (["P", "Q", "R", "S", "T"].includes(avatarLetter.value)) {
      letterAvatarColor.value = "text-warning";
    } else if (["U", "V", "W", "X", "Y"].includes(avatarLetter.value)) {
      letterAvatarColor.value = "text-info";
    } else {
      letterAvatarColor.value = "text-dark";
    }
  } else {
    // img_src =
    //   process.env.VUE_APP_API_URL +
    //   "/api/v1/client/media/" +
    //   url +
    //   "?access_token=" +
    //   JwtService.getToken();
    img_src = url;
  }
  return img_src;
};

src.value = processImageUrl(props.avatar);
</script>

<style lang="css">
.avatar-bg {
  background: url("/images/avatar.png") no-repeat;
  background-size: contain;
}
.avatar-bg img {
  object-fit: cover;
}
</style>
