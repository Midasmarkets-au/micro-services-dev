<template>
  <!--begin::Row-->
  <div class="row mb-5 mb-xl-10 mx-0">
    <div class="col-xl-4 mb-xl-10"></div>
    <div class="col-xl-4 mb-xl-10"></div>
    <div class="col-xl-4 mb-xl-10">
      <div class="card card-flush h-lg-100">
        <div class="card-body">
          <ul
            class="nav nav-pills nav-pills-custom row position-relative mx-0 mb-9"
          >
            <li class="nav-item col-4 mx-0 p-0">
              <a
                class="nav-link d-flex justify-content-center w-100 border-0 h-100"
                data-bs-toggle="pill"
                href="#dashboard-calendar"
              >
                <span class="nav-text text-gray-800 fw-bold fs-6 mb-3"
                  >Calendar</span
                >
                <span
                  class="bullet-custom position-absolute z-index-2 bottom-0 w-100 h-4px bg-primary rounded"
                ></span>
              </a>
            </li>
            <li class="nav-item col-4 mx-0 px-0">
              <a
                class="nav-link active d-flex justify-content-center w-100 border-0 h-100"
                data-bs-toggle="pill"
                href="#dashboard-notices"
              >
                <span class="nav-text text-gray-800 fw-bold fs-6 mb-3"
                  >Notices</span
                >
                <span
                  class="bullet-custom position-absolute z-index-2 bottom-0 w-100 h-4px bg-primary rounded"
                ></span>
              </a>
            </li>
          </ul>
          <div class="tab-content">
            <!--begin::Tap pane-->
            <div class="tab-pane fade show" id="dashboard-calendar">
              Calender
            </div>
            <div class="tab-pane fade show active" id="dashboard-notices">
              Notices
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
  <!--end::Row-->
</template>

<script lang="ts">
import { defineComponent } from "vue";

export default defineComponent({
  name: "main-dashboard",
  components: {},
});
</script>
