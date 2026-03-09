<template>
  <div class="sub-menu sub-h2 d-inline-flex" style="white-space: nowrap">
    <router-link to="/supports" class="sub-menu-item">{{
      $t("title.contactUs")
    }}</router-link>
    <router-link to="/supports/notices" class="sub-menu-item">{{
      $t("title.notices")
    }}</router-link>
    <router-link to="/supports/documents" class="sub-menu-item active">{{
      $t("title.documents")
    }}</router-link>
    <router-link
      to="/supports/cases"
      class="sub-menu-item"
      v-if="$cans(['TenantAdmin'])"
      >{{ $t("title.cases") }}</router-link
    >
  </div>
  <!--begin::Row-->
  <div v-if="region == 'au'" class="px-2 sm:px-0">
    <div class="card round-bl-br mb-2">
      <div class="card-header">
        <div class="card-title">
          <h3 class="d-flex align-items-center mx-1 m-0 fw-bold">
            {{ $t("title.disclosureDocument") }}
          </h3>
        </div>
      </div>
    </div>
    <div class="card card-body round-tl-tr">
      <div class="mx-0">
        <div class="row g-6 mb-6 mt-0">
          <!-- <p
            class="m-0"
            style="font-weight: 500; font-size: 20px; color: #212121"
          >
            {{ $t("title.disclosureDocument") }}
          </p> -->
          <div class="col-lg-3" v-for="(item, index) in baDocs" :key="index">
            <KTFile
              :file-title="$t('title.' + item.title)"
              file-type="pdf"
              :created-at="
                item.title == 'contractSpecifications'
                  ? $t('action.view')
                  : $t('action.clickDownload')
              "
              :file-link="getBaDocs(item.title)"
            ></KTFile>
          </div>
        </div>
      </div>
    </div>
  </div>
  <div v-else-if="region == 'jp'" class="px-2 sm:px-0">
    <div class="card round-bl-br mb-2">
      <div class="card-header">
        <div class="card-title">
          <h3 class="d-flex align-items-center mx-1 m-0 fw-bold">
            {{ $t("title.disclosureDocument") }}
          </h3>
        </div>
      </div>
    </div>
    <div class="card card-body round-tl-tr">
      <div class="mx-0">
        <div class="row g-6 mb-6 mt-0">
          <!-- <p
            class="m-0"
            style="font-weight: 500; font-size: 20px; color: #212121"
          >
            {{ $t("title.disclosureDocument") }}
          </p> -->
          <div class="col-lg-3" v-for="(item, index) in jpDocs" :key="index">
            <KTFile
              :file-title="item.title"
              file-type="pdf"
              :created-at="$t('action.clickDownload')"
              :file-link="item.src"
            ></KTFile>
          </div>
        </div>
      </div>
    </div>
  </div>
  <div v-else class="px-2 sm:px-0">
    <div class="card round-bl-br mb-2">
      <div class="card-header">
        <div class="card-title">
          <h3 class="d-flex align-items-center mx-1 m-0 fw-bold">
            {{ $t("title.disclosureDocument") }}
          </h3>
        </div>
      </div>
    </div>
    <div class="card card-body round-tl-tr">
      <div class="mx-0">
        <div class="row row-cols-1 row-cols-lg-5 g-6 mb-6 mt-0">
          <!-- <p
            class="m-0"
            style="font-weight: 500; font-size: 20px; color: #212121"
          >
            {{ $t("title.disclosureDocument") }}
          </p> -->
          <div class="col" v-for="(item, index) in bviDocs" :key="index">
            <KTFile
              :file-title="$t('title.' + item.title)"
              file-type="pdf"
              :created-at="
                item.title == 'contractSpecifications'
                  ? $t('action.view')
                  : $t('action.clickDownload')
              "
              :file-link="getBviDocs(item.title)"
            ></KTFile>
          </div>
        </div>
      </div>
    </div>
  </div>
  <!--end::Row-->
</template>

<script lang="ts" setup>
import "../assets/css/style.css";
import KTFile from "../components/DocFile.vue";
import { ref } from "vue";
import { useStore } from "@/store";
import {
  bviDocs,
  getBviDocs,
  baDocs,
  getBaDocs,
  jpDocs,
} from "@/core/data/bcrDocs";
const store = useStore();
const user = store.state.AuthModule.user;
const region = ref(user.tenancy);
if (region.value == null || region.value == undefined) {
  region.value = process.env.VUE_APP_SITE;
}
</script>
