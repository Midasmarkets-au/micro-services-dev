<template>
  <div class="mx-4" v-loading="transitionLoading">
    <div class="auth-box py-10">
      <div class="title">{{ $t("fields.oneTimeCode") }}</div>
      <div class="d-flex justify-content-center">
        <input
          v-model="code[0]"
          type="tel"
          name="pincode-1"
          maxlength="1"
          pattern="[\d]*"
          tabindex="1"
          autocomplete="off"
          @input="onCodeChange(0)"
          @keydown="(event) => onKeyDown(event, 0)"
          @paste="(event) => onPaste(event, 0)"
        />
        <input
          v-model="code[1]"
          type="tel"
          name="pincode-2"
          maxlength="1"
          pattern="[\d]*"
          tabindex="2"
          autocomplete="off"
          @input="onCodeChange(1)"
          @keydown="(event) => onKeyDown(event, 1)"
          @paste="(event) => onPaste(event, 1)"
        />
        <input
          v-model="code[2]"
          type="tel"
          name="pincode-3"
          maxlength="1"
          pattern="[\d]*"
          tabindex="3"
          autocomplete="off"
          @input="onCodeChange(2)"
          @keydown="(event) => onKeyDown(event, 2)"
          @paste="(event) => onPaste(event, 2)"
        />
        <input
          v-model="code[3]"
          type="tel"
          name="pincode-4"
          maxlength="1"
          pattern="[\d]*"
          tabindex="4"
          autocomplete="off"
          @input="onCodeChange(3)"
          @keydown="(event) => onKeyDown(event, 3)"
          @paste="(event) => onPaste(event, 3)"
        />
        <input
          v-model="code[4]"
          type="tel"
          name="pincode-5"
          maxlength="1"
          pattern="[\d]*"
          tabindex="5"
          autocomplete="off"
          @input="onCodeChange(4)"
          @keydown="(event) => onKeyDown(event, 4)"
          @paste="(event) => onPaste(event, 4)"
        />
        <input
          v-model="code[5]"
          type="tel"
          name="pincode-6"
          maxlength="1"
          pattern="[\d]*"
          tabindex="6"
          autocomplete="off"
          @input="onCodeChange(5)"
          @keydown="(event) => onKeyDown(event, 5)"
          @paste="(event) => onPaste(event, 5)"
        />
        <input
          v-model="code[6]"
          type="tel"
          name="pincode-7"
          maxlength="1"
          pattern="[\d]*"
          tabindex="7"
          autocomplete="off"
          @input="onCodeChange(6)"
          @keydown="(event) => onKeyDown(event, 6)"
          @paste="(event) => onPaste(event, 6)"
        />
      </div>
      <div class="mt-5 info">
        <p>
          An emaill with a verification code has been sent to your email. Enter
          the code to continue.
        </p>
      </div>
      <el-popover
        placement="top"
        :width="isMobile ? 320 : 400"
        trigger="click"
        v-model:visible="showPopover"
      >
        <template #reference>
          <div class="info-2">
            <span>Didn’t get a verification code?</span>
          </div>
        </template>
        <div v-loading="isLoading">
          <div class="row px-2 py-4">
            <div class="col-6 border-right">
              <div class="d-flex justify-content-center">
                <div class="text-center">
                  <el-icon :size="40" class=""><RefreshRight /></el-icon>
                  <p class="info-3" @click="resentCode">Resent Code</p>
                  <p>Get a new verification code.</p>
                </div>
              </div>
            </div>
            <div class="col-6">
              <div class="d-flex justify-content-center">
                <div class="text-center">
                  <el-icon :size="40" class=""><Message /></el-icon>
                  <p class="info-3" @click="step = 0">Change Email</p>
                  <p>Get a new verfication code with a different email.</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </el-popover>
    </div>
  </div>
</template>
<script lang="ts" setup>
import { ref, inject, watch } from "vue";
import { RefreshRight, Message } from "@element-plus/icons-vue";
import axios from "axios";
import { ElNotification } from "element-plus";
import { isMobile } from "@/core/config/WindowConfig";
const formData = inject("data");
const step = inject("step");
const code = ref(["", "", "", "", "", "", ""]);
const isLoading = ref(false);
const transitionLoading = ref(false);
const showPopover = ref(false);
const onCodeChange = (index: number) => {
  if (index < 6 && code.value[index] !== "") {
    const nextInput = document.getElementsByName(
      `pincode-${index + 2}`
    )[0] as HTMLInputElement;
    nextInput.focus();
  }
};
const onKeyDown = (event: KeyboardEvent, index: number) => {
  if (event.key === "Backspace" || event.key === "Delete") {
    if (index > 0 && code.value[index] == "") {
      event.preventDefault();
      const prevInput = document.getElementsByName(
        `pincode-${index}`
      )[0] as HTMLInputElement;
      prevInput.focus();
    }
  }
};

const onPaste = (event: ClipboardEvent, index: number) => {
  const paste = event.clipboardData?.getData("text") || "";
  if (paste.length > 0) {
    event.preventDefault();
    for (let i = 0; i < paste.length && index + i < code.value.length; i++) {
      code.value[index + i] = paste[i];
      const input = document.getElementsByName(
        `pincode-${index + i + 1}`
      )[0] as HTMLInputElement;
      input.value = paste[i];
    }
    if (index + paste.length < code.value.length) {
      const nextInput = document.getElementsByName(
        `pincode-${index + paste.length + 1}`
      )[0] as HTMLInputElement;
      nextInput.focus();
    }
  }
};

const resentCode = async (values) => {
  isLoading.value = true;
  try {
    await axios
      .post("/api/v2/auth/password-reset/code", {
        email: formData.value.email,
      })
      .then(() => {
        ElNotification({
          title: "Success",
          message: "Verification code resent successfully",
          type: "success",
        });
        code.value = ["", "", "", "", "", ""];
        showPopover.value = false;
      });
  } catch (error) {
    console.log(error);
    ElNotification({
      title: "Error",
      message: "Failed to resend verification code",
      type: "error",
    });
  }
  isLoading.value = false;
};

watch(
  code,
  (newCode) => {
    if (newCode.join("").length === 7) {
      transitionLoading.value = true;
      setTimeout(() => {
        formData.value.code = newCode.join("");
        step.value = 2;
        transitionLoading.value = false;
      }, 2000);
    }
  },
  { deep: true }
);
</script>
<style scoped>
input {
  width: 42px;
  height: 42px;
  border-radius: 8px;
  margin-right: 5px;
  border: 1px solid #86868b;
  text-align: center;
  font-size: 24px;
}
.info {
  font-size: 17px;
  line-height: 1.47059;
  text-align: center;
}
.info-2 {
  text-align: center;
  margin-top: 20px;
  font-size: 14px;
  font-weight: 400;
  letter-spacing: -0.016em;
  line-height: 1.42859;
  color: #06c;
  cursor: pointer;
}
.info-2:hover {
  text-decoration: underline;
}
.border-right {
  border-right: 1px solid #e5e5e5;
}
.info-3 {
  font-size: 14px;
  font-weight: 600;
  line-height: 1.42859;
  color: #06c;
  cursor: pointer;
}
.info-3:hover {
  text-decoration: underline;
}
</style>
