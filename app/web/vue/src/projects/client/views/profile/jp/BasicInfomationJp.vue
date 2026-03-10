<template>
  <div>
    <div class="card shadow-sm">
      <div class="card-header">
        <div class="card-title">{{ $t("title.basicInfo") }}</div>
      </div>
      <div class="card-body">
        <div :class="{ 'pe-10': !isMobile }">
          <div class="row">
            <div
              class="col-lg-4 d-flex flex-column align-items-center justify-content-center"
              :class="{ 'mb-5': isMobile }"
            >
              <div
                class="image-input image-input-outline w-125px h-125px"
                style="position: relative"
              >
                <AuthImage
                  :imageGuid="user.avatar"
                  :alt="user.name"
                  side="client"
                  class="image-input-wrapper w-125px h-125px"
                />

                <label
                  class="btn btn-icon btn-circle btn-active-color-primary w-25px h-25px bg-body shadow"
                  for="avatarInput"
                  title="Change avatar"
                  style="
                    position: absolute;
                    top: 0;
                    right: 0;
                    transform: translate(30%, -30%);
                  "
                >
                  <i class="bi bi-pencil-fill fs-7"></i>
                  <input
                    class="d-none"
                    id="avatarInput"
                    type="file"
                    name="avatar"
                    accept=".png, .jpg, .jpeg, .heic"
                    @change="avatarUpload"
                  />
                  <input type="hidden" name="avatar_remove" />
                </label>
              </div>
              <div class="form-text">
                {{ $t("tip.allowedFileTypes") }}: png, jpg, jpeg.
              </div>
            </div>

            <div class="col-lg-8">
              <div class="row">
                <div class="col-lg-12 mb-5">
                  <label for="" class="required mb-3">{{
                    $t("title.email")
                  }}</label>

                  <el-input
                    v-model="collectData.email"
                    size="large"
                    disabled
                    clearable
                  />
                </div>
                <div class="col-lg-4 mb-5">
                  <label for="" class="required mb-3">{{
                    $t("fields.nativeName")
                  }}</label>
                  <el-input
                    v-model="collectData.nativeName"
                    size="large"
                    clearable
                    disabled
                  />
                </div>

                <div class="col-lg-4 mb-5">
                  <label for="" class="required mb-3">{{
                    $t("fields.firstName")
                  }}</label>
                  <el-input
                    v-model="collectData.firstName"
                    size="large"
                    clearable
                    disabled
                  />
                </div>

                <div class="col-lg-4 mb-5">
                  <label for="" class="required mb-3">{{
                    $t("fields.lastName")
                  }}</label>
                  <el-input
                    v-model="collectData.lastName"
                    size="large"
                    clearable
                    disabled
                  />
                </div>
                <div class="col-lg-6 mb-5">
                  <label for="" class="required mb-3">{{
                    $t("fields.countryAndRegion")
                  }}</label>

                  <el-select
                    v-model="collectData.countryCode"
                    size="large"
                    disabled
                  >
                    <el-option
                      v-for="item in regionCodes"
                      :key="item.code"
                      :value="item.code"
                      :label="item.name"
                    />
                  </el-select>
                </div>
                <div class="col-lg-6 mb-5" v-if="!isMobile">
                  <label for="" class="required mb-3">{{
                    $t("fields.phoneNum")
                  }}</label>

                  <el-input
                    v-model="collectData.phoneNumber"
                    size="large"
                    :suffix-icon="EditPen"
                  >
                    <template #prepend>
                      <el-select
                        v-model="collectData.ccc"
                        size="large"
                        style="width: 190px"
                      >
                        <el-option
                          v-for="item in regionCodes"
                          :key="item.code"
                          :value="item.dialCode"
                          :label="item.name + ' + ' + item.dialCode"
                        />
                      </el-select>
                    </template>
                  </el-input>
                </div>

                <template v-if="isMobile">
                  <div class="col-lg-6 mb-5">
                    <label for="" class="required mb-3">{{
                      $t("fields.phoneNum")
                    }}</label>

                    <el-select size="large" v-model="collectData.ccc">
                      <el-option
                        v-for="item in regionCodes"
                        :key="item.code"
                        :value="item.dialCode"
                        :label="item.name + ' + ' + item.dialCode"
                      />
                    </el-select>
                  </div>

                  <div class="col-lg-6 mb-5">
                    <el-input
                      v-model="collectData.phoneNumber"
                      size="large"
                      :suffix-icon="EditPen"
                    >
                    </el-input>
                  </div>
                </template>

                <!-- <div class="col-lg-6 mb-5" v-if="showOtp">
                  <label for="" class="required mb-3">{{
                    $t("fields.oneTimeCode")
                  }}</label>

                  <el-input
                    v-model="collectData.otp"
                    clearable
                    :suffix-icon="EditPen"
                    size="large"
                  >
                    <template #append>
                      <el-button
                        @click.prevent="sendOtpCode"
                        :disabled="smsInterval !== 0"
                      >
                        <span v-if="smsInterval > 0">{{ smsInterval }} s</span>
                        <span v-else>{{ $t("action.sendCode") }}</span>
                      </el-button>
                    </template>
                  </el-input>
                </div> -->
              </div>
            </div>
          </div>
        </div>
        <div class="d-flex gap-5 flex-end mt-5">
          <button class="btn btn-sm btn-secondary" @click="resetData">
            {{ $t("action.reset") }}
          </button>
          <button
            class="btn btn-sm btn-primary"
            @click="submit"
            :disabled="isLoading"
          >
            <span v-if="isLoading">
              {{ $t("action.waiting") }}
              <span
                class="spinner-border h-15px w-15px align-middle text-gray-400"
              ></span>
            </span>
            <template v-else>{{ $t("action.saveChanges") }}</template>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import AuthImage from "@/components/AuthImage.vue";
import { getRegionCodes } from "@/core/data/phonesData";
import GlobalService from "@/projects/client/services/ClientGlobalService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { Actions } from "@/store/enums/StoreEnums";
import { nextTick, ref, watch } from "vue";
import { useStore } from "@/store";
import { EditPen } from "@element-plus/icons-vue";
import { useI18n } from "vue-i18n";
import { isMobile } from "@/core/config/WindowConfig";

// <el-icon><EditPen /></el-icon>
const store = useStore();
const user = store.state.AuthModule.user;
const regionCode = ref(user.countryCode.toLowerCase());
const isLoading = ref(false);
const regionCodes = ref(getRegionCodes());
const smsInterval = ref(0);
const { t } = useI18n();
const showOtp = ref(false);

const initialCollectData = {
  firstName: user.firstName,
  lastName: user.lastName,
  nativeName: user.nativeName,
  ccc: parseInt(user.ccc),
  phoneNumber: user.phoneNumber,
  email: user.email,
  countryCode: user.countryCode.toLowerCase(),
  language: user.language,
  currencyId: user.currencyId,
  timezone: user.timezone,
};

const deepCopy = JSON.parse(JSON.stringify(initialCollectData));
const collectData = ref<any>(deepCopy);

const resetData = () => {
  collectData.value = JSON.parse(JSON.stringify(initialCollectData));
};

const avatarUpload = async (event: any) => {
  if (event.target.files.item(0) == null) return;

  const file = event.target.files.item(0);
  const reader = new FileReader();
  reader.readAsDataURL(file);

  const form = new FormData();
  form.append("avatar", file);
  isLoading.value = true;

  try {
    const res = await GlobalService.uploadUserAvatar(form);
    await store.dispatch(Actions.SET_AVATAR, res.url);
    isLoading.value = false;
    // MsgPrompt.success();
  } catch (error) {
    MsgPrompt.error(error);
  }
};

// const discard = () => {
//   regionCode.value = originalPhoneData.value.regionCode;
//   collectData.value.ccc = originalPhoneData.value.ccc;
//   collectData.value.phoneNumber = originalPhoneData.value.phoneNumber;
//   collectData.value.otp = "";
// };

// const sendOtpCode = async () => {
//   //
//   try {
//     await GlobalService.sendVerificationOneTimeCodeForPhone(
//       collectData.value.ccc,
//       collectData.value.phoneNumber
//     );
//     MsgPrompt.success(
//       t("tip.verificationCodeSendTo") +
//         " +" +
//         collectData.value.ccc +
//         " " +
//         collectData.value.phoneNumber
//     );
//     smsInterval.value = 60;
//     const interval = setInterval(() => {
//       smsInterval.value--;
//       if (smsInterval.value <= 0) clearInterval(interval);
//     }, 1000);
//   } catch (error) {
//     MsgPrompt.error(error);
//   }
// };

watch(
  [() => collectData.value.ccc, () => collectData.value.phoneNumber],
  () => {
    showOtp.value =
      collectData.value.ccc != user.ccc ||
      collectData.value.phoneNumber != user.phoneNumber;
  }
);

const submit = async () => {
  //
  isLoading.value = true;
  try {
    if (showOtp.value) {
      // if (!collectData.value.otp || collectData.value.otp === "") {
      //   MsgPrompt.error(t("tip.oneTimeCodeVerificationFail"));
      //   return;
      // }
      await GlobalService.verifyPhoneNumberChange(
        collectData.value.ccc,
        collectData.value.phoneNumber,
        "undefined"
      );
      MsgPrompt.success(t("tip.profileUpdated"));
      collectData.value.otp = "";
      showOtp.value = false;
    } else {
      MsgPrompt.warning(t("tip.nothingToUpdate"));
    }
    // if (
    //   collectData.value.firstName === user.firstName &&
    //   collectData.value.lastName === user.lastName
    // ) {
    //   MsgPrompt.error(t("tip.nothingToUpdate"));
    //   return;
    // }
    // await GlobalService.updateUserProfile({
    //   firstName: collectData.value.firstName,
    //   lastName: collectData.value.lastName,
    // });
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};
</script>

<style scoped></style>
