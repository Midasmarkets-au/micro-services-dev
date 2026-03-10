<template>
  <div class="m-6">
    <el-tag type="success" size="large" class="fs-4 me-3">
      Case ID: {{ data.caseId }}</el-tag
    >
    <el-tag type="success" size="large" class="fs-4 me-3"
      >Category: {{ $t("title." + data.categoryName) }}</el-tag
    >
    <el-tag type="success" size="large" class="fs-4 me-3"
      >Status: {{ CaseStatusTypes[data.status] }}</el-tag
    >
  </div>
  <div class="mx-3">
    <div class="row border">
      <div class="col-2 border-end p-4">
        <div class="d-flex justify-content-center mb-3">
          <UserAvatar
            :avatar="user.avatar"
            :name="user.name"
            side="client"
            size="100px"
            rounded
          ></UserAvatar>
        </div>
        <div class="text-center">{{ user.name }}</div>
      </div>
      <div class="col-10 p-4 position-relative">
        <div>{{ data.content }}</div>
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
              :avatar="user.avatar"
              :name="user.name"
              side="client"
              size="100px"
              rounded
            ></UserAvatar>
            <div class="text-center">{{ user.name }}</div>
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
  <ReplyCase :item="item" @replied="onReplied" />
</template>
<script setup lang="ts">
import ReplyCase from "./ReplyCase.vue";
import CaseImages from "./CaseImages.vue";
import { ref, inject, onMounted } from "vue";
import SupportService from "../../services/SupportService";
import { CaseStatusTypes } from "@/core/types/SupportStatusTypes";
import { useStore } from "@/store";
import { computed } from "vue";
const store = useStore();
const user = computed<any>(() => store.state.AuthModule.user);
const isLoading = ref(false);
const item = inject<any>("item");
const data = ref<any>({});
const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await SupportService.getCaseById(item.value.caseId);
    data.value = res;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const onReplied = () => {
  fetchData();
};
onMounted(() => {
  fetchData();
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
