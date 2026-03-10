import { Ref } from "vue";

export function useAddressFinder(inputRef: Ref<HTMLInputElement | null>) {
  let widget: any = null;

  const initAddressFinder = () => {
    if (inputRef.value) {
      console.log("start initAddressFinder", inputRef.value);
      const apiKey = process.env.VUE_APP_AU_ADDRESS_FINDER_KEY;
      widget = new (window as any).AddressFinder.Widget(
        inputRef.value,
        apiKey,
        "AU",
        {
          address_params: {
            source: "gnaf,paf",
          },
        }
      );

      widget.on("result:select", (fullAddress: string, metaData: any) => {
        if (inputRef.value) {
          inputRef.value.value = metaData.full_address; // Update the DOM element
          const inputEvent = new Event("input", { bubbles: true });
          inputRef.value.dispatchEvent(inputEvent); // Trigger the v-model to update
        }
      });
    }
  };

  const downloadAddressFinder = () => {
    const script = document.createElement("script");
    script.src = "https://api.addressfinder.io/assets/v3/widget.js";
    script.async = true;
    script.onload = initAddressFinder;
    document.head.appendChild(script);
  };
  downloadAddressFinder();
}
