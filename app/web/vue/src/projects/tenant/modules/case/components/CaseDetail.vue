<template>
  <el-drawer v-model="drawer" direction="ltr" size="90%">
    <template #header>
      <div>
        <el-tag type="success" size="large" class="fs-4 me-3">
          Case ID: {{ data.caseId }}</el-tag
        >
        <el-tag type="success" size="large" class="fs-4 me-3"
          >Category: {{ $t("title." + data.categoryName) }}</el-tag
        >
        <el-tag type="success" size="large" class="fs-4 me-3"
          >Status: {{ CaseStatusTypes[data.status] }}</el-tag
        >
        <el-tag type="success" size="large" class="fs-4 me-3">
          <TimeShow :date-iso-string="data.createdOn"
        /></el-tag>
        <el-tag
          v-if="data.claimedAdminName != ''"
          type="success"
          size="large"
          class="fs-4 me-3"
        >
          Claimed By: {{ data.claimedAdminName }}</el-tag
        >
      </div>
      <el-button class="me-4" @click="showTranslation = !showTranslation"
        ><span v-if="showTranslation == false"> Show Translation</span>
        <span v-else>Hide Translation</span>
      </el-button>
    </template>
    <div class="mx-3">
      <div class="row border">
        <div class="col-2 border-end p-4">
          <div class="d-flex justify-content-center mb-3">
            <UserAvatar
              avatar=""
              name="User"
              side="client"
              size="100px"
              rounded
            ></UserAvatar>
          </div>
          <div class="text-center">User</div>
        </div>
        <div class="col-10 p-4 position-relative">
          <div>{{ data.content }}</div>

          <div
            class="card mt-4 ps-4 pt-2"
            v-if="showTranslation && data.languages?.length != 0"
          >
            <div v-for="(languageObject, index) in data.languages" :key="index">
              <p>
                {{ Object.keys(languageObject)[0] }}:
                {{ Object.values(languageObject)[0] }}
              </p>
            </div>
          </div>
          <div class="mt-10">
            <TranslateText
              :text="data.content"
              :case="data"
              @translated="fetchData(id)"
            />
          </div>
          <el-divider v-if="data.files?.length != 0" />
          <div class="d-flex gap-3 mt-5">
            <CaseImages
              v-for="(img, index) in data.files"
              :key="index"
              :guid="img"
            />
          </div>

          <div class="time">
            <TimeShow :date-iso-string="data.createdOn" type="reportTime" />
          </div>
        </div>
      </div>
    </div>
    <div v-for="(item, index) in data.replies" :key="index" class="mx-3">
      <div class="row border" :class="item.isAdmin == true ? 'bg-light' : ''">
        <div class="col-2 border-end p-4">
          <div class="d-flex justify-content-center mb-3">
            <div v-if="item.isAdmin == false">
              <UserAvatar
                avatar=""
                name="user"
                side="client"
                size="100px"
                rounded
              ></UserAvatar>
              <div class="text-center">User</div>
            </div>
            <div v-else>
              <UserAvatar
                avatar=""
                :name="data.claimedAdminName"
                side="admin"
                size="100px"
                rounded
              ></UserAvatar>
              <div class="text-center">{{ data.claimedAdminName }}</div>
            </div>
          </div>
        </div>
        <div class="col-10 p-4 position-relative">
          <div>{{ item.content }}</div>
          <div
            class="card mt-4 ps-4 pt-2"
            v-if="showTranslation && item.languages?.length != 0"
          >
            <div v-for="(languageObject, index) in item.languages" :key="index">
              <p>
                {{ Object.keys(languageObject)[0] }}:
                {{ Object.values(languageObject)[0] }}
              </p>
            </div>
          </div>
          <div class="mt-10">
            <TranslateText
              :text="item.content"
              :case="item"
              @translated="fetchData(id)"
            />
          </div>
          <el-divider v-if="item.files?.length != 0" />
          <div class="d-flex gap-3 mt-5">
            <CaseImages
              v-for="(img, index) in item.files"
              :key="index"
              :guid="img"
            />
          </div>
          <div class="time">
            <TimeShow :date-iso-string="item.createdOn" type="reportTime" />
          </div>
        </div>
      </div>
    </div>
    <el-divider></el-divider>

    <ReplyCase :item="data" @replied="replied" />
  </el-drawer>
</template>
<script setup lang="ts">
import { ref } from "vue";
import SupportService from "../services/SupportService";
import { CaseStatusTypes } from "@/core/types/SupportStatusTypes";
import ReplyCase from "./ReplyCase.vue";
import CaseImages from "./CaseImages.vue";
import TranslateText from "./TranslateText.vue";
import { computed } from "vue";
const isLoading = ref(false);
const drawer = ref(false);
const data = ref(<any>[]);
const title = ref("");
const id = computed(() => data.value.id);
const showTranslation = ref(true);
const show = (_item: any) => {
  drawer.value = true;
  fetchData(_item.id);
};

const fetchData = async (id: number) => {
  isLoading.value = true;
  try {
    const res = await SupportService.queryCaseById(id);
    data.value = res;
    title.value = res.caseId;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const replied = () => {
  fetchData(data.value.id);
};

defineExpose({
  show,
});
</script>
<style scoped>
.border-around {
  border: 1px solid #000;
}
.border-right {
  border-right: 1px solid #000;
}
.demo-image__error .image-slot {
  font-size: 30px;
}
.demo-image__error .image-slot .el-icon {
  font-size: 30px;
}
.demo-image__error .el-image {
  width: 100%;
  height: 200px;
}

.time {
  font-size: 12px;
  color: rgba(113, 113, 113, 1);
  position: absolute;
  bottom: 10px;
  right: 10px;
}
</style>
